//************************************************************************//
//  physx_physics_server_3d.cpp                                           //
//************************************************************************//
//                         This file is part of:                          //
//                            KaizenEngine                                //
//              https://github.com/CeoMatheusDev/Kaizen                   //
//************************************************************************//
// Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  //
// Copyright (c) 2014-present Godot Engine contributors.                  //
// Copyright (c) 2026 Matheus A. Santos, Hardion Labs.                    //
//                                                                        //
// This file contains code derived from the Godot Engine                  //
// (https://godotengine.org), which is licensed under the MIT License.    //
// The original MIT notice is preserved below.                            //
//                                                                        //
// Modified by Matheus A. Santos, Hardion Labs on 2026-11-23.             //
// Modifications and additional code are licensed under the GNU Affero    //
// General Public License v3.0 (AGPL-3.0).                                //
// See the files LICENSE.txt and LICENSE-ADDITIONAL-TERMS.md for details. //
//************************************************************************//

#include "physx_physics_server_3d.h"

#include "core/config/project_settings.h"
#include "core/math/geometry_3d.h"
#include "core/os/mutex.h"

#include <cooking/PxCooking.h>
#include <extensions/PxRigidBodyExt.h>

using namespace physx;

// ---------------------------------------------------------------------------
// Query filtering and cooking helpers.
// ---------------------------------------------------------------------------

namespace {

// Query filter: layer/mask + exclude set.
class PhysXQueryFilter : public PxQueryFilterCallback {
public:
	uint32_t collision_mask = UINT32_MAX;
	HashSet<const void *> exclude;
	bool collide_bodies = true;
	bool collide_areas = false;

	virtual PxQueryHitType::Enum preFilter(const PxFilterData &filterData, const PxShape *shape, const PxRigidActor *actor, PxHitFlags &) override {
		if (!actor || !shape) {
			return PxQueryHitType::eNONE;
		}
		const void *owner = actor->userData;
		if (exclude.has(owner)) {
			return PxQueryHitType::eNONE;
		}
		const bool is_trigger = shape->getFlags() & PxShapeFlag::eTRIGGER_SHAPE;
		if (is_trigger && !collide_areas) {
			return PxQueryHitType::eNONE;
		}
		if (!is_trigger && !collide_bodies) {
			return PxQueryHitType::eNONE;
		}
		if (!(filterData.word0 & collision_mask)) {
			return PxQueryHitType::eNONE;
		}
		return PxQueryHitType::eBLOCK;
	}
	virtual PxQueryHitType::Enum postFilter(const PxFilterData &, const PxQueryHit &, const PxShape *, const PxRigidActor *) override {
		return PxQueryHitType::eBLOCK;
	}
};

// Convert engine shape data to a PhysX geometry; cooks meshes on demand.
bool _cook_convex(PxConvexMesh *&r_mesh, const Vector<Vector3> &p_points, PxPhysics *p_physics) {
	if (!p_physics || p_points.is_empty()) {
		return false;
	}
	PxConvexMeshDesc desc;
	desc.points.count = p_points.size();
	desc.points.stride = sizeof(Vector3);
	desc.points.data = p_points.ptr();
	desc.flags = PxConvexFlag::eCOMPUTE_CONVEX;
	PxDefaultMemoryOutputStream out;
	if (!PxCookConvexMesh(PxCookingParams(PxTolerancesScale()), desc, out)) {
		return false;
	}
	PxDefaultMemoryInputData in(out.getData(), out.getSize());
	r_mesh = p_physics->createConvexMesh(in);
	return r_mesh != nullptr;
}

} // namespace

// ---------------------------------------------------------------------------
// Direct states.
// ---------------------------------------------------------------------------

class PhysXPhysicsServer3D::PhysXDirectBodyState : public PhysicsDirectBodyState3DDummy {
public:
	PhysXDirectBodyState() :
			PhysicsDirectBodyState3DDummy(nullptr) {}

	PhysXBody *body = nullptr;

	virtual Transform3D get_transform() const override {
		return body && body->actor ? PhysXBackend::from_px(body->actor->getGlobalPose()) : Transform3D();
	}
	virtual Vector3 get_linear_velocity() const override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		return dyn ? PhysXBackend::from_px(dyn->getLinearVelocity()) : Vector3();
	}
	virtual Vector3 get_angular_velocity() const override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		return dyn ? PhysXBackend::from_px(dyn->getAngularVelocity()) : Vector3();
	}
	virtual real_t get_inverse_mass() const override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		return dyn && dyn->getMass() > 0 ? 1.0 / dyn->getMass() : 0.0;
	}
	virtual Vector3 get_total_gravity() const override {
		return body ? (real_t)body->params[PS3DE::BODY_PARAM_GRAVITY_SCALE] * Vector3(0, -9.81, 0) : Vector3();
	}
	virtual real_t get_total_angular_damp() const override {
		return body ? (real_t)body->params[PS3DE::BODY_PARAM_ANGULAR_DAMP] : (real_t)0;
	}
	virtual real_t get_total_linear_damp() const override {
		return body ? (real_t)body->params[PS3DE::BODY_PARAM_LINEAR_DAMP] : (real_t)0;
	}
	virtual Vector3 get_center_of_mass() const override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		return dyn ? get_transform().xform(PhysXBackend::from_px(dyn->getCMassLocalPose().p)) : Vector3();
	}
	virtual Vector3 get_center_of_mass_local() const override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		return dyn ? PhysXBackend::from_px(dyn->getCMassLocalPose().p) : Vector3();
	}
	virtual void set_linear_velocity(const Vector3 &p_velocity) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			dyn->setLinearVelocity(PhysXBackend::to_px(p_velocity));
		}
	}
	virtual void set_angular_velocity(const Vector3 &p_velocity) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			dyn->setAngularVelocity(PhysXBackend::to_px(p_velocity));
		}
	}
	virtual void set_transform(const Transform3D &p_transform) override {
		if (body && body->actor) {
			body->actor->setGlobalPose(PhysXBackend::to_px(p_transform));
		}
	}
	virtual Vector3 get_velocity_at_local_position(const Vector3 &p_position) const override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (!dyn) {
			return Vector3();
		}
		const Vector3 rel = get_transform().basis.xform(p_position) - get_center_of_mass();
		return get_linear_velocity() + get_angular_velocity().cross(rel);
	}
	virtual void apply_central_impulse(const Vector3 &p_impulse) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			dyn->addForce(PhysXBackend::to_px(p_impulse), PxForceMode::eIMPULSE);
		}
	}
	virtual void apply_impulse(const Vector3 &p_impulse, const Vector3 &p_position = Vector3()) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			const PxTransform pose = dyn->getGlobalPose();
			PxRigidBodyExt::addForceAtPos(*dyn, PhysXBackend::to_px(p_impulse), pose.transform(PhysXBackend::to_px(p_position)), PxForceMode::eIMPULSE);
		}
	}
	virtual void apply_torque_impulse(const Vector3 &p_impulse) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			dyn->addTorque(PhysXBackend::to_px(p_impulse), PxForceMode::eIMPULSE);
		}
	}
	virtual void apply_central_force(const Vector3 &p_force) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			dyn->addForce(PhysXBackend::to_px(p_force), PxForceMode::eFORCE);
		}
	}
	virtual void apply_force(const Vector3 &p_force, const Vector3 &p_position = Vector3()) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			const PxTransform pose = dyn->getGlobalPose();
			PxRigidBodyExt::addForceAtPos(*dyn, PhysXBackend::to_px(p_force), pose.transform(PhysXBackend::to_px(p_position)), PxForceMode::eFORCE);
		}
	}
	virtual void apply_torque(const Vector3 &p_torque) override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		if (dyn) {
			dyn->addTorque(PhysXBackend::to_px(p_torque), PxForceMode::eFORCE);
		}
	}
	virtual bool is_sleeping() const override {
		PxRigidDynamic *dyn = body ? body->actor ? body->actor->is<PxRigidDynamic>() : nullptr : nullptr;
		return dyn ? dyn->isSleeping() : true;
	}
	virtual int get_contact_count() const override { return 0; }
};

class PhysXPhysicsServer3D::PhysXDirectSpaceState : public PhysicsDirectSpaceState3DDummy {
public:
	PhysXPhysicsServer3D *server = nullptr;
	PhysXSpace *space = nullptr;

	virtual bool intersect_ray(const PS3DT::RayParameters &p_parameters, PS3DT::RayResult &r_result) override {
		if (!space || !space->scene) {
			return false;
		}
		PhysXQueryFilter filter;
		filter.collision_mask = p_parameters.collision_mask;
		filter.collide_bodies = p_parameters.collide_with_bodies;
		filter.collide_areas = p_parameters.collide_with_areas;
		for (const RID &E : p_parameters.exclude) {
			PhysXBody *body = server->body_owner.get_or_null(E);
			if (body) {
				filter.exclude.insert(body);
				continue;
			}
			PhysXArea *area = server->area_owner.get_or_null(E);
			if (area) {
				filter.exclude.insert(area);
			}
		}
		PxRaycastBuffer buffer;
		PxQueryFilterData qdata;
		qdata.flags = PxQueryFlag::eSTATIC | PxQueryFlag::eDYNAMIC | PxQueryFlag::ePREFILTER;
		const Vector3 dir = p_parameters.to - p_parameters.from;
		const real_t len = dir.length();
		if (len <= 0) {
			return false;
		}
		if (!space->scene->raycast(PhysXBackend::to_px(p_parameters.from), PhysXBackend::to_px(dir / len), len, buffer, PxHitFlag::eDEFAULT, qdata, &filter)) {
			return false;
		}
		const PxRaycastHit &hit = buffer.block;
		r_result.position = PhysXBackend::from_px(hit.position);
		r_result.normal = PhysXBackend::from_px(hit.normal);
		if (hit.actor) {
			PhysXPhysicsServer3D::PhysXObject *owner = (PhysXPhysicsServer3D::PhysXObject *)hit.actor->userData;
			if (owner) {
				r_result.rid = owner->rid;
				r_result.collider_id = owner->instance_id;
			}
		}
		return true;
	}

	virtual int intersect_point(const PS3DT::PointParameters &p_parameters, PS3DT::ShapeResult *r_results, int p_result_max) override {
		if (!space || !space->scene || p_result_max <= 0) {
			return 0;
		}
		PhysXQueryFilter filter;
		filter.collision_mask = p_parameters.collision_mask;
		filter.collide_bodies = p_parameters.collide_with_bodies;
		filter.collide_areas = p_parameters.collide_with_areas;
		for (const RID &E : p_parameters.exclude) {
			PhysXBody *body = server->body_owner.get_or_null(E);
			if (body) {
				filter.exclude.insert(body);
			}
			PhysXArea *area = server->area_owner.get_or_null(E);
			if (area) {
				filter.exclude.insert(area);
			}
		}
		PxOverlapBuffer buffer;
		PxQueryFilterData qdata;
		qdata.flags = PxQueryFlag::eSTATIC | PxQueryFlag::eDYNAMIC | PxQueryFlag::ePREFILTER;
		const PxSphereGeometry sphere(0.001f);
		const PxTransform pose = PxTransform(PhysXBackend::to_px(p_parameters.position));
		if (!space->scene->overlap(sphere, pose, buffer, qdata, &filter)) {
			return 0;
		}
		int count = 0;
		for (PxU32 i = 0; i < buffer.nbTouches && count < p_result_max; i++) {
			const PxOverlapHit &hit = buffer.touches[i];
			if (!hit.actor) {
				continue;
			}
			PhysXPhysicsServer3D::PhysXObject *owner = (PhysXPhysicsServer3D::PhysXObject *)hit.actor->userData;
			if (!owner) {
				continue;
			}
			r_results[count].rid = owner->rid;
			r_results[count].collider_id = owner->instance_id;
			r_results[count].shape = hit.shape ? (int)(uintptr_t)hit.shape->userData : 0;
			count++;
		}
		return count;
	}

	virtual int intersect_shape(const PS3DT::ShapeParameters &p_parameters, PS3DT::ShapeResult *r_results, int p_result_max) override {
		if (!space || !space->scene || p_result_max <= 0) {
			return 0;
		}
		PhysXShape *shape = server->shape_owner.get_or_null(p_parameters.shape_rid);
		if (!shape || !shape->valid) {
			return 0;
		}
		PhysXQueryFilter filter;
		filter.collision_mask = p_parameters.collision_mask;
		filter.collide_bodies = p_parameters.collide_with_bodies;
		filter.collide_areas = p_parameters.collide_with_areas;
		for (const RID &E : p_parameters.exclude) {
			PhysXBody *body = server->body_owner.get_or_null(E);
			if (body) {
				filter.exclude.insert(body);
			}
			PhysXArea *area = server->area_owner.get_or_null(E);
			if (area) {
				filter.exclude.insert(area);
			}
		}
		PxOverlapBuffer buffer;
		PxQueryFilterData qdata;
		qdata.flags = PxQueryFlag::eSTATIC | PxQueryFlag::eDYNAMIC | PxQueryFlag::ePREFILTER;
		const PxTransform pose = PxTransform(PhysXBackend::to_px(p_parameters.transform.origin));
		if (!space->scene->overlap(shape->geometry.any(), pose, buffer, qdata, &filter)) {
			return 0;
		}
		int count = 0;
		for (PxU32 i = 0; i < buffer.nbTouches && count < p_result_max; i++) {
			const PxOverlapHit &hit = buffer.touches[i];
			if (!hit.actor) {
				continue;
			}
			PhysXPhysicsServer3D::PhysXObject *owner = (PhysXPhysicsServer3D::PhysXObject *)hit.actor->userData;
			if (!owner) {
				continue;
			}
			r_results[count].rid = owner->rid;
			r_results[count].collider_id = owner->instance_id;
			count++;
		}
		return count;
	}

	virtual bool collide_shape(const PS3DT::ShapeParameters &p_parameters, Vector3 *r_results, int p_result_max, int &r_result_count) override {
		r_result_count = 0;
		if (!space || !space->scene || p_result_max <= 0) {
			return false;
		}
		PhysXShape *shape = server->shape_owner.get_or_null(p_parameters.shape_rid);
		if (!shape || !shape->valid) {
			return false;
		}
		PhysXQueryFilter filter;
		filter.collision_mask = p_parameters.collision_mask;
		filter.collide_bodies = p_parameters.collide_with_bodies;
		filter.collide_areas = p_parameters.collide_with_areas;
		for (const RID &E : p_parameters.exclude) {
			PhysXBody *body = server->body_owner.get_or_null(E);
			if (body) {
				filter.exclude.insert(body);
			}
			PhysXArea *area = server->area_owner.get_or_null(E);
			if (area) {
				filter.exclude.insert(area);
			}
		}
		PxOverlapBuffer buffer;
		PxQueryFilterData qdata;
		qdata.flags = PxQueryFlag::eSTATIC | PxQueryFlag::eDYNAMIC | PxQueryFlag::ePREFILTER;
		const PxTransform pose = PxTransform(PhysXBackend::to_px(p_parameters.transform.origin));
		if (!space->scene->overlap(shape->geometry.any(), pose, buffer, qdata, &filter)) {
			return false;
		}
		for (PxU32 i = 0; i < buffer.nbTouches && r_result_count < p_result_max; i++) {
			const PxOverlapHit &hit = buffer.touches[i];
			if (hit.actor) {
				PhysXPhysicsServer3D::PhysXObject *owner = (PhysXPhysicsServer3D::PhysXObject *)hit.actor->userData;
				if (owner) {
					r_results[r_result_count++] = PhysXBackend::from_px(hit.actor->getGlobalPose().p);
				}
			}
		}
		return r_result_count > 0;
	}

	virtual bool rest_info(const PS3DT::ShapeParameters &p_parameters, PS3DT::ShapeRestInfo *r_info) override {
		if (!space || !space->scene || !r_info) {
			return false;
		}
		PhysXShape *shape = server->shape_owner.get_or_null(p_parameters.shape_rid);
		if (!shape || !shape->valid) {
			return false;
		}
		PhysXQueryFilter filter;
		filter.collision_mask = p_parameters.collision_mask;
		filter.collide_bodies = p_parameters.collide_with_bodies;
		filter.collide_areas = p_parameters.collide_with_areas;
		PxOverlapBuffer buffer;
		PxQueryFilterData qdata;
		qdata.flags = PxQueryFlag::eSTATIC | PxQueryFlag::eDYNAMIC | PxQueryFlag::ePREFILTER;
		const PxTransform pose = PxTransform(PhysXBackend::to_px(p_parameters.transform.origin));
		if (!space->scene->overlap(shape->geometry.any(), pose, buffer, qdata, &filter) || buffer.nbTouches == 0) {
			return false;
		}
		const PxOverlapHit &hit = buffer.touches[0];
		if (!hit.actor) {
			return false;
		}
		PhysXPhysicsServer3D::PhysXObject *owner = (PhysXPhysicsServer3D::PhysXObject *)hit.actor->userData;
		if (!owner) {
			return false;
		}
		r_info->point = PhysXBackend::from_px(hit.actor->getGlobalPose().p);
		r_info->normal = Vector3(0, 1, 0);
		r_info->rid = owner->rid;
		r_info->collider_id = owner->instance_id;
		return true;
	}
};

// ---------------------------------------------------------------------------
// Shape cooking.
// ---------------------------------------------------------------------------

PhysXPhysicsServer3D::PhysXShape::~PhysXShape() {
	if (convex_mesh) {
		convex_mesh->release();
	}
	if (triangle_mesh) {
		triangle_mesh->release();
	}
	if (height_field) {
		height_field->release();
	}
}

bool PhysXPhysicsServer3D::_build_shape_geometry(PhysXShape *p_shape) {
	PhysXPhysicsServer3D::PhysXShape &s = *p_shape;
	s.valid = true;
	switch (s.type) {
		case PS3DE::SHAPE_WORLD_BOUNDARY: {
			s.geometry.storeAny(PxPlaneGeometry());
		} break;
		case PS3DE::SHAPE_SEPARATION_RAY: {
			// Thin box approximation (PhysX has no ray shape).
			const real_t len = (real_t)s.data;
			s.geometry.storeAny(PxBoxGeometry(0.001f, (float)(len * 0.5), 0.001f));
		} break;
		case PS3DE::SHAPE_SPHERE: {
			s.geometry.storeAny(PxSphereGeometry((float)(real_t)s.data));
		} break;
		case PS3DE::SHAPE_BOX: {
			const Vector3 e = s.data;
			s.geometry.storeAny(PxBoxGeometry((float)e.x, (float)e.y, (float)e.z));
		} break;
		case PS3DE::SHAPE_CAPSULE:
		case PS3DE::SHAPE_CYLINDER: {
			const Dictionary d = s.data;
			const real_t radius = d.has("radius") ? (real_t)d["radius"] : 0.5;
			const real_t height = d.has("height") ? (real_t)d["height"] : 1.0;
			if (s.type == PS3DE::SHAPE_CAPSULE) {
				s.geometry.storeAny(PxCapsuleGeometry((float)radius, (float)(height * 0.5)));
			} else {
				// Cylinder = convex hull of a prism (PhysX has no cylinder primitive).
				Vector<Vector3> points;
				const int sides = 8;
				for (int i = 0; i < sides; i++) {
					const real_t a = Math::TAU * i / sides;
					const Vector3 p(Math::cos(a) * radius, -height * 0.5, Math::sin(a) * radius);
					points.push_back(p);
					points.push_back(Vector3(p.x, height * 0.5, p.z));
				}
				if (!_cook_convex(s.convex_mesh, points, backend->get_physics())) {
					s.valid = false;
					return false;
				}
				s.geometry.storeAny(PxConvexMeshGeometry(s.convex_mesh));
			}
		} break;
		case PS3DE::SHAPE_CONVEX_POLYGON: {
			const Vector<Vector3> points = s.data;
			if (!_cook_convex(s.convex_mesh, points, backend->get_physics())) {
				s.valid = false;
				return false;
			}
			s.geometry.storeAny(PxConvexMeshGeometry(s.convex_mesh));
		} break;
		case PS3DE::SHAPE_CONCAVE_POLYGON: {
			Vector<Vector3> vertices;
			Vector<int> indices;
			const Variant &v = s.data;
			if (v.get_type() == Variant::DICTIONARY) {
				const Dictionary d = v;
				vertices = d.has("triangles") ? (Vector<Vector3>)d["triangles"] : Vector<Vector3>();
				indices = d.has("indices") ? (Vector<int>)d["indices"] : Vector<int>();
			} else if (v.get_type() == Variant::PACKED_VECTOR3_ARRAY) {
				vertices = v;
			}
			if (vertices.is_empty()) {
				s.valid = false;
				return false;
			}
			if (indices.is_empty()) {
				indices.resize(vertices.size());
				for (int i = 0; i < vertices.size(); i++) {
					indices.ptrw()[i] = i;
				}
			}
			PxTriangleMeshDesc desc;
			desc.points.count = vertices.size();
			desc.points.stride = sizeof(Vector3);
			desc.points.data = vertices.ptr();
			desc.triangles.count = indices.size() / 3;
			desc.triangles.stride = sizeof(int32_t) * 3;
			desc.triangles.data = indices.ptr();
			PxDefaultMemoryOutputStream out;
			if (!PxCookTriangleMesh(PxCookingParams(PxTolerancesScale()), desc, out)) {
				s.valid = false;
				return false;
			}
			PxDefaultMemoryInputData in(out.getData(), out.getSize());
			s.triangle_mesh = backend->get_physics()->createTriangleMesh(in);
			if (!s.triangle_mesh) {
				s.valid = false;
				return false;
			}
			s.geometry.storeAny(PxTriangleMeshGeometry(s.triangle_mesh));
		} break;
		case PS3DE::SHAPE_HEIGHTMAP: {
			const Dictionary d = s.data;
			const int width = d.has("width") ? (int)d["width"] : 0;
			const int depth = d.has("depth") ? (int)d["depth"] : 0;
			const Vector<real_t> heights = d.has("heights") ? (Vector<real_t>)d["heights"] : Vector<real_t>();
			if (width < 2 || depth < 2 || heights.size() < width * depth) {
				s.valid = false;
				return false;
			}
			Vector<PxHeightFieldSample> samples;
			samples.resize((width - 1) * (depth - 1));
			for (int z = 0; z < depth - 1; z++) {
				for (int x = 0; x < width - 1; x++) {
					PxHeightFieldSample &sample = samples.ptrw()[z * (width - 1) + x];
					sample.height = (PxI16)CLAMP(heights[z * width + x], -32767.0, 32767.0);
					sample.materialIndex0 = 0;
					sample.materialIndex1 = 0;
					sample.clearTessFlag();
				}
			}
			PxHeightFieldDesc desc;
			desc.format = PxHeightFieldFormat::eS16_TM;
			desc.nbRows = depth - 1;
			desc.nbColumns = width - 1;
			desc.samples.data = samples.ptr();
			desc.samples.stride = sizeof(PxHeightFieldSample);
			PxDefaultMemoryOutputStream out;
			if (!PxCookHeightField(desc, out)) {
				s.valid = false;
				return false;
			}
			PxDefaultMemoryInputData in(out.getData(), out.getSize());
			s.height_field = backend->get_physics()->createHeightField(in);
			if (!s.height_field) {
				s.valid = false;
				return false;
			}
			s.geometry.storeAny(PxHeightFieldGeometry(s.height_field, PxMeshGeometryFlags(), 1.0f, 1.0f, 1.0f));
		} break;
		case PS3DE::SHAPE_SOFT_BODY:
		case PS3DE::SHAPE_CUSTOM: {
			s.valid = false;
			return false;
		}
	}
	return s.valid;
}

// ---------------------------------------------------------------------------
// Filter data / shape attach helpers.
// ---------------------------------------------------------------------------

PxFilterData PhysXPhysicsServer3D::_make_filter_data(uint32_t p_layer, uint32_t p_mask, const PhysXObject *p_object) const {
	PxFilterData data;
	data.word0 = p_layer;
	data.word1 = p_mask;
	const uint64_t key = (uint64_t)(uintptr_t)p_object;
	data.word2 = (uint32_t)(key & 0xffffffffu);
	data.word3 = (uint32_t)(key >> 32);
	return data;
}

RID PhysXPhysicsServer3D::_alloc_shape(PS3DE::ShapeType p_type) {
	const RID rid = shape_owner.make_rid();
	PhysXShape *s = shape_owner.get_or_null(rid);
	s->rid = rid;
	s->type = p_type;
	return rid;
}

void PhysXPhysicsServer3D::_body_add_px_shape(PhysXBody *p_body, int p_idx) {
	if (!p_body->actor || p_idx < 0 || p_idx >= p_body->shapes.size()) {
		return;
	}
	ShapeInstance &inst = p_body->shapes.ptrw()[p_idx];
	PhysXShape *s = shape_owner.get_or_null(inst.shape_rid);
	if (!s || !s->valid) {
		return;
	}
	PxPhysics *px = backend->get_physics();
	PxMaterial *material = px->createMaterial(
			(float)p_body->params[PS3DE::BODY_PARAM_FRICTION],
			(float)p_body->params[PS3DE::BODY_PARAM_FRICTION],
			(float)p_body->params[PS3DE::BODY_PARAM_BOUNCE]);
	PxShape *shape = PxRigidActorExt::createExclusiveShape(*p_body->actor, s->geometry.any(), *material);
	if (!shape) {
		return;
	}
	shape->setLocalPose(PhysXBackend::to_px(inst.local_transform));
	shape->setSimulationFilterData(_make_filter_data(p_body->collision_layer, p_body->collision_mask, p_body));
	shape->setQueryFilterData(_make_filter_data(p_body->collision_layer, p_body->collision_mask, p_body));
	shape->setFlag(PxShapeFlag::eSCENE_QUERY_SHAPE, !inst.disabled);
	shape->userData = (void *)(uintptr_t)p_idx;
	inst.px_shape = shape;
}

void PhysXPhysicsServer3D::_body_remove_px_shape(PhysXBody *p_body, int p_idx) {
	if (!p_body->actor || p_idx < 0 || p_idx >= p_body->shapes.size()) {
		return;
	}
	ShapeInstance &inst = p_body->shapes.ptrw()[p_idx];
	if (inst.px_shape) {
		p_body->actor->detachShape(*inst.px_shape, true);
		inst.px_shape = nullptr;
	}
}

void PhysXPhysicsServer3D::_body_rebuild_shapes(PhysXBody *p_body) {
	if (!p_body->actor) {
		return;
	}
	for (int i = 0; i < p_body->shapes.size(); i++) {
		_body_remove_px_shape(p_body, i);
	}
	for (int i = 0; i < p_body->shapes.size(); i++) {
		_body_add_px_shape(p_body, i);
	}
}

void PhysXPhysicsServer3D::_area_add_px_shape(PhysXArea *p_area, int p_idx) {
	if (!p_area->actor || p_idx < 0 || p_idx >= p_area->shapes.size()) {
		return;
	}
	ShapeInstance &inst = p_area->shapes.ptrw()[p_idx];
	PhysXShape *s = shape_owner.get_or_null(inst.shape_rid);
	if (!s || !s->valid) {
		return;
	}
	PxPhysics *px = backend->get_physics();
	PxMaterial *material = px->createMaterial(0.5f, 0.5f, 0.5f);
	PxShape *shape = PxRigidActorExt::createExclusiveShape(*p_area->actor, s->geometry.any(), *material);
	if (!shape) {
		return;
	}
	shape->setLocalPose(PhysXBackend::to_px(inst.local_transform));
	shape->setSimulationFilterData(_make_filter_data(p_area->collision_layer, p_area->collision_mask, p_area));
	shape->setQueryFilterData(_make_filter_data(p_area->collision_layer, p_area->collision_mask, p_area));
	shape->setFlag(PxShapeFlag::eSIMULATION_SHAPE, false);
	shape->setFlag(PxShapeFlag::eSCENE_QUERY_SHAPE, !inst.disabled);
	shape->userData = (void *)(uintptr_t)p_idx;
	inst.px_shape = shape;
}

void PhysXPhysicsServer3D::_area_remove_px_shape(PhysXArea *p_area, int p_idx) {
	if (!p_area->actor || p_idx < 0 || p_idx >= p_area->shapes.size()) {
		return;
	}
	ShapeInstance &inst = p_area->shapes.ptrw()[p_idx];
	if (inst.px_shape) {
		p_area->actor->detachShape(*inst.px_shape, true);
		inst.px_shape = nullptr;
	}
}

void PhysXPhysicsServer3D::_area_rebuild_shapes(PhysXArea *p_area) {
	if (!p_area->actor) {
		return;
	}
	for (int i = 0; i < p_area->shapes.size(); i++) {
		_area_remove_px_shape(p_area, i);
	}
	for (int i = 0; i < p_area->shapes.size(); i++) {
		_area_add_px_shape(p_area, i);
	}
}

void PhysXPhysicsServer3D::_apply_body_mode(PhysXBody *p_body) {
	// Recreate the actor with the right class for the mode.
	PxPhysics *px = backend->get_physics();
	if (!px) {
		return;
	}
	const Transform3D pose = p_body->actor ? PhysXBackend::from_px(p_body->actor->getGlobalPose()) : Transform3D();
	RID space = p_body->space;
	PhysXSpace *sp = space_owner.get_or_null(space);
	if (p_body->actor) {
		if (sp && sp->scene) {
			sp->scene->removeActor(*p_body->actor);
		}
		p_body->actor->release();
		p_body->actor = nullptr;
	}
	for (int i = 0; i < p_body->shapes.size(); i++) {
		p_body->shapes.ptrw()[i].px_shape = nullptr;
	}

	switch (p_body->mode) {
		case PS3DE::BODY_MODE_STATIC: {
			p_body->actor = px->createRigidStatic(PhysXBackend::to_px(pose));
		} break;
		case PS3DE::BODY_MODE_KINEMATIC: {
			PxRigidDynamic *dyn = px->createRigidDynamic(PhysXBackend::to_px(pose));
			dyn->setRigidBodyFlag(PxRigidBodyFlag::eKINEMATIC, true);
			p_body->actor = dyn;
		} break;
		case PS3DE::BODY_MODE_RIGID:
		case PS3DE::BODY_MODE_RIGID_LINEAR: {
			PxRigidDynamic *dyn = px->createRigidDynamic(PhysXBackend::to_px(pose));
			if (p_body->mode == PS3DE::BODY_MODE_RIGID_LINEAR) {
				dyn->setRigidDynamicLockFlags(PxRigidDynamicLockFlag::eLOCK_ANGULAR_X | PxRigidDynamicLockFlag::eLOCK_ANGULAR_Y | PxRigidDynamicLockFlag::eLOCK_ANGULAR_Z);
			}
			p_body->actor = dyn;
		} break;
	}
	if (!p_body->actor) {
		return;
	}
	p_body->actor->userData = p_body;
	_body_rebuild_shapes(p_body);
	_apply_body_mass(p_body);
	_apply_filter_data(p_body);
	if (sp && sp->scene) {
		sp->scene->addActor(*p_body->actor);
	}
}

void PhysXPhysicsServer3D::_apply_body_mass(PhysXBody *p_body) {
	PxRigidDynamic *dyn = p_body->actor ? p_body->actor->is<PxRigidDynamic>() : nullptr;
	if (!dyn || p_body->mode == PS3DE::BODY_MODE_KINEMATIC) {
		return;
	}
	const real_t mass = (real_t)p_body->params[PS3DE::BODY_PARAM_MASS];
	if (mass > 0) {
		PxRigidBodyExt::setMassAndUpdateInertia(*dyn, (float)mass);
	} else {
		PxRigidBodyExt::updateMassAndInertia(*dyn, 1.0f);
	}
	if (p_body->param_set[PS3DE::BODY_PARAM_INERTIA]) {
		const Vector3 inertia = p_body->params[PS3DE::BODY_PARAM_INERTIA];
		dyn->setMassSpaceInertiaTensor(PhysXBackend::to_px(inertia).abs());
	}
}

void PhysXPhysicsServer3D::_apply_filter_data(PhysXBody *p_body) {
	if (!p_body->actor) {
		return;
	}
	const PxU32 count = p_body->actor->getNbShapes();
	PxShape **shapes = (PxShape **)alloca(sizeof(PxShape *) * MAX(count, (PxU32)1));
	p_body->actor->getShapes(shapes, count);
	const PxFilterData data = _make_filter_data(p_body->collision_layer, p_body->collision_mask, p_body);
	for (PxU32 i = 0; i < count; i++) {
		shapes[i]->setSimulationFilterData(data);
		shapes[i]->setQueryFilterData(data);
	}
}

void PhysXPhysicsServer3D::_destroy_joint(PhysXJoint *p_joint) {
	if (p_joint->px_joint) {
		p_joint->px_joint->release();
		p_joint->px_joint = nullptr;
	}
	p_joint->body_a = RID();
	p_joint->body_b = RID();
}

void PhysXPhysicsServer3D::_check_area_overlaps() {
	// Per-step overlap diffing: covers body<->area and area<->area monitors.
	for (const KeyValue<RID, PhysXArea *> &E : area_map) {
		PhysXArea *area = E.value;
		if (!area->actor || !area->monitorable) {
			continue;
		}
		PhysXSpace *sp = space_owner.get_or_null(area->space);
		if (!sp || !sp->scene) {
			continue;
		}

		HashMap<RID, LocalVector<int>> new_bodies;
		HashMap<RID, LocalVector<int>> new_areas;

		PhysXQueryFilter filter;
		filter.collision_mask = area->collision_mask;
		filter.collide_bodies = true;
		filter.collide_areas = true;
		filter.exclude.insert(area);

		PxOverlapBuffer buffer;
		PxQueryFilterData qdata;
		qdata.flags = PxQueryFlag::eSTATIC | PxQueryFlag::eDYNAMIC | PxQueryFlag::ePREFILTER;
		for (int i = 0; i < area->shapes.size(); i++) {
			const ShapeInstance &inst = area->shapes[i];
			PhysXShape *s = shape_owner.get_or_null(inst.shape_rid);
			if (!s || !s->valid || inst.disabled) {
				continue;
			}
			const PxTransform pose = area->actor->getGlobalPose() * PhysXBackend::to_px(inst.local_transform);
			if (!sp->scene->overlap(s->geometry.any(), pose, buffer, qdata, &filter)) {
				continue;
			}
			for (PxU32 t = 0; t < buffer.nbTouches; t++) {
				const PxOverlapHit &hit = buffer.touches[t];
				if (!hit.actor) {
					continue;
				}
				PhysXPhysicsServer3D::PhysXObject *owner = (PhysXPhysicsServer3D::PhysXObject *)hit.actor->userData;
				if (!owner || owner == area) {
					continue;
				}
				int shape_idx = 0;
				if (hit.shape) {
					shape_idx = (int)(uintptr_t)hit.shape->userData;
				}
				if (owner->kind == PhysXPhysicsServer3D::PhysXObject::BODY) {
					new_bodies[owner->rid].push_back(shape_idx);
				} else {
					new_areas[owner->rid].push_back(shape_idx);
				}
			}
		}

		// Diff and dispatch monitor callbacks.
		auto dispatch_diff = [&](HashMap<RID, LocalVector<int>> &p_old, const HashMap<RID, LocalVector<int>> &p_new, bool p_is_area) {
			for (const KeyValue<RID, LocalVector<int>> &E2 : p_new) {
				if (!p_old.has(E2.key) && (p_is_area ? area->area_monitor_callback.is_valid() : area->monitor_callback.is_valid())) {
					const Callable &cb = p_is_area ? area->area_monitor_callback : area->monitor_callback;
					ObjectID id;
					if (p_is_area) {
						PhysXArea *const *pp_a = area_map.getptr(E2.key); PhysXArea *other = pp_a ? *pp_a : nullptr;
						id = other ? other->instance_id : ObjectID();
					} else {
						PhysXBody *const *pp_b = body_map.getptr(E2.key); PhysXBody *other = pp_b ? *pp_b : nullptr;
						id = other ? other->object_id : ObjectID();
					}
					cb.call(E2.key, id, E2.value.size() ? (int)E2.value[0] : -1, 0, true);
				}
			}
			for (const KeyValue<RID, LocalVector<int>> &E2 : p_old) {
				if (!p_new.has(E2.key) && (p_is_area ? area->area_monitor_callback.is_valid() : area->monitor_callback.is_valid())) {
					const Callable &cb = p_is_area ? area->area_monitor_callback : area->monitor_callback;
					ObjectID id;
					if (p_is_area) {
						PhysXArea *const *pp_a = area_map.getptr(E2.key); PhysXArea *other = pp_a ? *pp_a : nullptr;
						id = other ? other->instance_id : ObjectID();
					} else {
						PhysXBody *const *pp_b = body_map.getptr(E2.key); PhysXBody *other = pp_b ? *pp_b : nullptr;
						id = other ? other->object_id : ObjectID();
					}
					cb.call(E2.key, id, E2.value.size() ? (int)E2.value[0] : -1, 0, false);
				}
			}
		};
		dispatch_diff(area->overlapping_bodies, new_bodies, false);
		dispatch_diff(area->overlapping_areas, new_areas, true);
		area->overlapping_bodies = new_bodies;
		area->overlapping_areas = new_areas;
	}
}

// ---------------------------------------------------------------------------
// Construction.
// ---------------------------------------------------------------------------

PhysXPhysicsServer3D::PhysXPhysicsServer3D() {
	backend.instantiate();
}

PhysXPhysicsServer3D::~PhysXPhysicsServer3D() {
	finish();
}

// ---------------------------------------------------------------------------
// Shapes.
// ---------------------------------------------------------------------------

RID PhysXPhysicsServer3D::world_boundary_shape_create() {
	return _alloc_shape(PS3DE::SHAPE_WORLD_BOUNDARY);
}

RID PhysXPhysicsServer3D::separation_ray_shape_create() {
	const RID rid = _alloc_shape(PS3DE::SHAPE_SEPARATION_RAY);
	shape_owner.get_or_null(rid)->data = 1.0f;
	return rid;
}

RID PhysXPhysicsServer3D::sphere_shape_create() {
	const RID rid = _alloc_shape(PS3DE::SHAPE_SPHERE);
	shape_owner.get_or_null(rid)->data = 0.5f;
	return rid;
}

RID PhysXPhysicsServer3D::box_shape_create() {
	const RID rid = _alloc_shape(PS3DE::SHAPE_BOX);
	shape_owner.get_or_null(rid)->data = Vector3(0.5, 0.5, 0.5);
	return rid;
}

RID PhysXPhysicsServer3D::capsule_shape_create() {
	const RID rid = _alloc_shape(PS3DE::SHAPE_CAPSULE);
	Dictionary d;
	d["radius"] = 0.5f;
	d["height"] = 1.0f;
	shape_owner.get_or_null(rid)->data = d;
	return rid;
}

RID PhysXPhysicsServer3D::cylinder_shape_create() {
	const RID rid = _alloc_shape(PS3DE::SHAPE_CYLINDER);
	Dictionary d;
	d["radius"] = 0.5f;
	d["height"] = 1.0f;
	shape_owner.get_or_null(rid)->data = d;
	return rid;
}

RID PhysXPhysicsServer3D::convex_polygon_shape_create() {
	return _alloc_shape(PS3DE::SHAPE_CONVEX_POLYGON);
}

RID PhysXPhysicsServer3D::concave_polygon_shape_create() {
	return _alloc_shape(PS3DE::SHAPE_CONCAVE_POLYGON);
}

RID PhysXPhysicsServer3D::heightmap_shape_create() {
	return _alloc_shape(PS3DE::SHAPE_HEIGHTMAP);
}

void PhysXPhysicsServer3D::shape_set_data(RID p_shape, const Variant &p_data) {
	PhysXShape *s = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(s);
	s->data = p_data;
	if (s->convex_mesh) {
		s->convex_mesh->release();
		s->convex_mesh = nullptr;
	}
	if (s->triangle_mesh) {
		s->triangle_mesh->release();
		s->triangle_mesh = nullptr;
	}
	if (s->height_field) {
		s->height_field->release();
		s->height_field = nullptr;
	}
	_build_shape_geometry(s);
}

void PhysXPhysicsServer3D::shape_set_custom_solver_bias(RID p_shape, real_t p_bias) {
	PhysXShape *s = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(s);
	s->custom_solver_bias = p_bias;
}

PS3DE::ShapeType PhysXPhysicsServer3D::shape_get_type(RID p_shape) const {
	const PhysXShape *s = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL_V(s, PS3DE::SHAPE_SPHERE);
	return s->type;
}

Variant PhysXPhysicsServer3D::shape_get_data(RID p_shape) const {
	const PhysXShape *s = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL_V(s, Variant());
	return s->data;
}

void PhysXPhysicsServer3D::shape_set_margin(RID p_shape, real_t p_margin) {
	PhysXShape *s = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(s);
	s->margin = p_margin;
}

real_t PhysXPhysicsServer3D::shape_get_margin(RID p_shape) const {
	const PhysXShape *s = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL_V(s, 0);
	return s->margin;
}

real_t PhysXPhysicsServer3D::shape_get_custom_solver_bias(RID p_shape) const {
	const PhysXShape *s = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL_V(s, 0);
	return s->custom_solver_bias;
}

// ---------------------------------------------------------------------------
// Spaces.
// ---------------------------------------------------------------------------

RID PhysXPhysicsServer3D::space_create() {
	const RID rid = space_owner.make_rid();
	PhysXSpace *sp = space_owner.get_or_null(rid);
	sp->rid = rid;
	sp->scene = (PxScene *)backend->create_scene(sp->gravity);
	if (!default_space.is_valid()) {
		default_space = rid;
	}
	return rid;
}

void PhysXPhysicsServer3D::space_set_active(RID p_space, bool p_active) {
	PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL(sp);
	sp->active = p_active;
}

bool PhysXPhysicsServer3D::space_is_active(RID p_space) const {
	const PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(sp, false);
	return sp->active;
}

void PhysXPhysicsServer3D::space_set_param(RID p_space, PS3DE::SpaceParameter p_param, real_t p_value) {
	PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL(sp);
	sp->params[p_param] = p_value;
	// PhysX contact offsets are per-shape; the space-level value is cached only.
}

real_t PhysXPhysicsServer3D::space_get_param(RID p_space, PS3DE::SpaceParameter p_param) const {
	const PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(sp, 0);
	const HashMap<int, real_t>::ConstIterator it = sp->params.find(p_param);
	return it ? it->value : 0;
}

PhysicsDirectSpaceState3D *PhysXPhysicsServer3D::space_get_direct_state(RID p_space) {
	PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(sp, nullptr);
	if (!direct_space_states.has(p_space)) {
		PhysXDirectSpaceState *state = memnew(PhysXDirectSpaceState);
		state->server = this;
		state->space = sp;
		direct_space_states[p_space] = state;
	}
	PhysXDirectSpaceState *state = direct_space_states[p_space];
	state->server = this;
	state->space = sp;
	return state;
}

void PhysXPhysicsServer3D::space_set_debug_contacts(RID p_space, int p_max_contacts) {
	PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL(sp);
	sp->max_debug_contacts = p_max_contacts;
}

Vector<Vector3> PhysXPhysicsServer3D::space_get_contacts(RID p_space) const {
	const PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(sp, Vector<Vector3>());
	return sp->debug_contacts;
}

int PhysXPhysicsServer3D::space_get_contact_count(RID p_space) const {
	const PhysXSpace *sp = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(sp, 0);
	return sp->debug_contacts.size();
}

// ---------------------------------------------------------------------------
// Areas.
// ---------------------------------------------------------------------------

RID PhysXPhysicsServer3D::area_create() {
	const RID rid = area_owner.make_rid();
	PhysXArea *area = area_owner.get_or_null(rid);
	area->rid = rid;
	area_map[rid] = area;
	PxPhysics *px = backend->get_physics();
	if (px) {
		area->actor = px->createRigidStatic(PxTransform(PxIdentity));
		if (area->actor) {
			area->actor->userData = area;
		}
	}
	return rid;
}

void PhysXPhysicsServer3D::area_set_space(RID p_area, RID p_space) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	PhysXSpace *old_sp = space_owner.get_or_null(area->space);
	if (old_sp && old_sp->scene && area->actor) {
		old_sp->scene->removeActor(*area->actor);
	}
	area->space = p_space;
	PhysXSpace *sp = space_owner.get_or_null(p_space);
	if (sp && sp->scene && area->actor) {
		sp->scene->addActor(*area->actor);
	}
}

RID PhysXPhysicsServer3D::area_get_space(RID p_area) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, RID());
	return area->space;
}

void PhysXPhysicsServer3D::area_add_shape(RID p_area, RID p_shape, const Transform3D &p_transform, bool p_disabled) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	ShapeInstance inst;
	inst.shape_rid = p_shape;
	inst.local_transform = p_transform;
	inst.disabled = p_disabled;
	area->shapes.push_back(inst);
	_area_add_px_shape(area, area->shapes.size() - 1);
}

void PhysXPhysicsServer3D::area_set_shape(RID p_area, int p_shape_idx, RID p_shape) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	ERR_FAIL_INDEX(p_shape_idx, area->shapes.size());
	_area_remove_px_shape(area, p_shape_idx);
	area->shapes.ptrw()[p_shape_idx].shape_rid = p_shape;
	_area_add_px_shape(area, p_shape_idx);
}

void PhysXPhysicsServer3D::area_set_shape_transform(RID p_area, int p_shape_idx, const Transform3D &p_transform) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	ERR_FAIL_INDEX(p_shape_idx, area->shapes.size());
	area->shapes.ptrw()[p_shape_idx].local_transform = p_transform;
	if (area->shapes[p_shape_idx].px_shape) {
		area->shapes[p_shape_idx].px_shape->setLocalPose(PhysXBackend::to_px(p_transform));
	}
}

int PhysXPhysicsServer3D::area_get_shape_count(RID p_area) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, 0);
	return area->shapes.size();
}

RID PhysXPhysicsServer3D::area_get_shape(RID p_area, int p_shape_idx) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, RID());
	ERR_FAIL_INDEX_V(p_shape_idx, area->shapes.size(), RID());
	return area->shapes[p_shape_idx].shape_rid;
}

Transform3D PhysXPhysicsServer3D::area_get_shape_transform(RID p_area, int p_shape_idx) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, Transform3D());
	ERR_FAIL_INDEX_V(p_shape_idx, area->shapes.size(), Transform3D());
	return area->shapes[p_shape_idx].local_transform;
}

void PhysXPhysicsServer3D::area_remove_shape(RID p_area, int p_shape_idx) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	ERR_FAIL_INDEX(p_shape_idx, area->shapes.size());
	_area_remove_px_shape(area, p_shape_idx);
	area->shapes.remove_at(p_shape_idx);
}

void PhysXPhysicsServer3D::area_clear_shapes(RID p_area) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	for (int i = area->shapes.size() - 1; i >= 0; i--) {
		_area_remove_px_shape(area, i);
	}
	area->shapes.clear();
}

void PhysXPhysicsServer3D::area_set_shape_disabled(RID p_area, int p_shape_idx, bool p_disabled) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	ERR_FAIL_INDEX(p_shape_idx, area->shapes.size());
	area->shapes.ptrw()[p_shape_idx].disabled = p_disabled;
	if (area->shapes[p_shape_idx].px_shape) {
		area->shapes[p_shape_idx].px_shape->setFlag(PxShapeFlag::eSCENE_QUERY_SHAPE, !p_disabled);
	}
}

void PhysXPhysicsServer3D::area_attach_object_instance_id(RID p_area, ObjectID p_id) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->instance_id = p_id;
}

ObjectID PhysXPhysicsServer3D::area_get_object_instance_id(RID p_area) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, ObjectID());
	return area->instance_id;
}

void PhysXPhysicsServer3D::area_set_param(RID p_area, PS3DE::AreaParameter p_param, const Variant &p_value) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	switch (p_param) {
		case PS3DE::AREA_PARAM_GRAVITY: {
			area->gravity = p_value;
		} break;
		case PS3DE::AREA_PARAM_GRAVITY_VECTOR: {
			area->gravity_vector = p_value;
		} break;
		case PS3DE::AREA_PARAM_GRAVITY_IS_POINT: {
			area->gravity_is_point = p_value;
		} break;
		case PS3DE::AREA_PARAM_GRAVITY_POINT_UNIT_DISTANCE: {
			area->gravity_point_unit_distance = p_value;
		} break;
		case PS3DE::AREA_PARAM_LINEAR_DAMP: {
			area->linear_damp = p_value;
		} break;
		case PS3DE::AREA_PARAM_ANGULAR_DAMP: {
			area->angular_damp = p_value;
		} break;
		case PS3DE::AREA_PARAM_PRIORITY: {
			area->priority = p_value;
		} break;
		default: {
			// Wind and override modes: cached only (documented limitation).
		} break;
	}
}

void PhysXPhysicsServer3D::area_set_transform(RID p_area, const Transform3D &p_transform) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	if (area->actor) {
		area->actor->setGlobalPose(PhysXBackend::to_px(p_transform));
	}
}

Variant PhysXPhysicsServer3D::area_get_param(RID p_parea, PS3DE::AreaParameter p_param) const {
	const PhysXArea *area = area_owner.get_or_null(p_parea);
	ERR_FAIL_NULL_V(area, Variant());
	switch (p_param) {
		case PS3DE::AREA_PARAM_GRAVITY: {
			return area->gravity;
		} break;
		case PS3DE::AREA_PARAM_GRAVITY_VECTOR: {
			return area->gravity_vector;
		} break;
		case PS3DE::AREA_PARAM_GRAVITY_IS_POINT: {
			return area->gravity_is_point;
		} break;
		case PS3DE::AREA_PARAM_GRAVITY_POINT_UNIT_DISTANCE: {
			return area->gravity_point_unit_distance;
		} break;
		case PS3DE::AREA_PARAM_LINEAR_DAMP: {
			return area->linear_damp;
		} break;
		case PS3DE::AREA_PARAM_ANGULAR_DAMP: {
			return area->angular_damp;
		} break;
		case PS3DE::AREA_PARAM_PRIORITY: {
			return area->priority;
		} break;
		default: {
			return Variant();
		}
	}
}

Transform3D PhysXPhysicsServer3D::area_get_transform(RID p_area) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, Transform3D());
	return area->actor ? PhysXBackend::from_px(area->actor->getGlobalPose()) : Transform3D();
}

void PhysXPhysicsServer3D::area_set_collision_layer(RID p_area, uint32_t p_layer) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->collision_layer = p_layer;
	_area_rebuild_shapes(area);
}

uint32_t PhysXPhysicsServer3D::area_get_collision_layer(RID p_area) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, 0);
	return area->collision_layer;
}

void PhysXPhysicsServer3D::area_set_collision_mask(RID p_area, uint32_t p_mask) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->collision_mask = p_mask;
	_area_rebuild_shapes(area);
}

uint32_t PhysXPhysicsServer3D::area_get_collision_mask(RID p_area) const {
	const PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, 0);
	return area->collision_mask;
}

void PhysXPhysicsServer3D::area_set_monitorable(RID p_area, bool p_monitorable) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->monitorable = p_monitorable;
}

void PhysXPhysicsServer3D::area_set_monitor_callback(RID p_area, const Callable &p_callback) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->monitor_callback = p_callback;
}

void PhysXPhysicsServer3D::area_set_area_monitor_callback(RID p_area, const Callable &p_callback) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->area_monitor_callback = p_callback;
}

void PhysXPhysicsServer3D::area_set_ray_pickable(RID p_area, bool p_enable) {
	PhysXArea *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->ray_pickable = p_enable;
}

// ---------------------------------------------------------------------------
// Bodies.
// ---------------------------------------------------------------------------

RID PhysXPhysicsServer3D::body_create() {
	const RID rid = body_owner.make_rid();
	PhysXBody *body = body_owner.get_or_null(rid);
	body->rid = rid;
	body->params[PS3DE::BODY_PARAM_MASS] = 1.0;
	body->params[PS3DE::BODY_PARAM_FRICTION] = 1.0;
	body->params[PS3DE::BODY_PARAM_BOUNCE] = 0.0;
	body->params[PS3DE::BODY_PARAM_GRAVITY_SCALE] = 1.0;
	body_map[rid] = body;
	_apply_body_mode(body);
	return rid;
}

void PhysXPhysicsServer3D::body_set_space(RID p_body, RID p_space) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PhysXSpace *old_sp = space_owner.get_or_null(body->space);
	if (old_sp && old_sp->scene && body->actor) {
		old_sp->scene->removeActor(*body->actor);
	}
	body->space = p_space;
	PhysXSpace *sp = space_owner.get_or_null(p_space);
	if (sp && sp->scene && body->actor) {
		sp->scene->addActor(*body->actor);
	}
}

RID PhysXPhysicsServer3D::body_get_space(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, RID());
	return body->space;
}

void PhysXPhysicsServer3D::body_set_mode(RID p_body, PS3DE::BodyMode p_mode) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	if (body->mode == p_mode) {
		return;
	}
	body->mode = p_mode;
	_apply_body_mode(body);
}

PS3DE::BodyMode PhysXPhysicsServer3D::body_get_mode(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, PS3DE::BODY_MODE_STATIC);
	return body->mode;
}

void PhysXPhysicsServer3D::body_add_shape(RID p_body, RID p_shape, const Transform3D &p_transform, bool p_disabled) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	ShapeInstance inst;
	inst.shape_rid = p_shape;
	inst.local_transform = p_transform;
	inst.disabled = p_disabled;
	body->shapes.push_back(inst);
	_body_add_px_shape(body, body->shapes.size() - 1);
	_apply_body_mass(body);
}

void PhysXPhysicsServer3D::body_set_shape(RID p_body, int p_shape_idx, RID p_shape) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	ERR_FAIL_INDEX(p_shape_idx, body->shapes.size());
	_body_remove_px_shape(body, p_shape_idx);
	body->shapes.ptrw()[p_shape_idx].shape_rid = p_shape;
	_body_add_px_shape(body, p_shape_idx);
	_apply_body_mass(body);
}

void PhysXPhysicsServer3D::body_set_shape_transform(RID p_body, int p_shape_idx, const Transform3D &p_transform) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	ERR_FAIL_INDEX(p_shape_idx, body->shapes.size());
	body->shapes.ptrw()[p_shape_idx].local_transform = p_transform;
	if (body->shapes[p_shape_idx].px_shape) {
		body->shapes[p_shape_idx].px_shape->setLocalPose(PhysXBackend::to_px(p_transform));
	}
}

int PhysXPhysicsServer3D::body_get_shape_count(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return body->shapes.size();
}

RID PhysXPhysicsServer3D::body_get_shape(RID p_body, int p_shape_idx) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, RID());
	ERR_FAIL_INDEX_V(p_shape_idx, body->shapes.size(), RID());
	return body->shapes[p_shape_idx].shape_rid;
}

Transform3D PhysXPhysicsServer3D::body_get_shape_transform(RID p_body, int p_shape_idx) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Transform3D());
	ERR_FAIL_INDEX_V(p_shape_idx, body->shapes.size(), Transform3D());
	return body->shapes[p_shape_idx].local_transform;
}

void PhysXPhysicsServer3D::body_remove_shape(RID p_body, int p_shape_idx) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	ERR_FAIL_INDEX(p_shape_idx, body->shapes.size());
	_body_remove_px_shape(body, p_shape_idx);
	body->shapes.remove_at(p_shape_idx);
	_apply_body_mass(body);
}

void PhysXPhysicsServer3D::body_clear_shapes(RID p_body) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	for (int i = body->shapes.size() - 1; i >= 0; i--) {
		_body_remove_px_shape(body, i);
	}
	body->shapes.clear();
}

void PhysXPhysicsServer3D::body_set_shape_disabled(RID p_body, int p_shape_idx, bool p_disabled) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	ERR_FAIL_INDEX(p_shape_idx, body->shapes.size());
	body->shapes.ptrw()[p_shape_idx].disabled = p_disabled;
	if (body->shapes[p_shape_idx].px_shape) {
		body->shapes[p_shape_idx].px_shape->setFlag(PxShapeFlag::eSCENE_QUERY_SHAPE, !p_disabled);
		body->shapes[p_shape_idx].px_shape->setFlag(PxShapeFlag::eSIMULATION_SHAPE, !p_disabled);
	}
}

void PhysXPhysicsServer3D::body_attach_object_instance_id(RID p_body, ObjectID p_id) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->object_id = p_id;
	body->instance_id = p_id;
}

ObjectID PhysXPhysicsServer3D::body_get_object_instance_id(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, ObjectID());
	return body->object_id;
}

void PhysXPhysicsServer3D::body_set_enable_continuous_collision_detection(RID p_body, bool p_enable) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->ccd = p_enable;
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		dyn->setRigidBodyFlag(PxRigidBodyFlag::eENABLE_CCD, p_enable);
	}
}

bool PhysXPhysicsServer3D::body_is_continuous_collision_detection_enabled(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);
	return body->ccd;
}

void PhysXPhysicsServer3D::body_set_collision_layer(RID p_body, uint32_t p_layer) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->collision_layer = p_layer;
	_apply_filter_data(body);
}

uint32_t PhysXPhysicsServer3D::body_get_collision_layer(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return body->collision_layer;
}

void PhysXPhysicsServer3D::body_set_collision_mask(RID p_body, uint32_t p_mask) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->collision_mask = p_mask;
	_apply_filter_data(body);
}

uint32_t PhysXPhysicsServer3D::body_get_collision_mask(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return body->collision_mask;
}

void PhysXPhysicsServer3D::body_set_collision_priority(RID p_body, real_t p_priority) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->collision_priority = p_priority;
}

real_t PhysXPhysicsServer3D::body_get_collision_priority(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return body->collision_priority;
}

void PhysXPhysicsServer3D::body_set_user_flags(RID p_body, uint32_t p_flags) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->user_flags = p_flags;
}

uint32_t PhysXPhysicsServer3D::body_get_user_flags(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return body->user_flags;
}

void PhysXPhysicsServer3D::body_set_param(RID p_body, PS3DE::BodyParameter p_param, const Variant &p_value) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	ERR_FAIL_INDEX((int)p_param, (int)PS3DE::BODY_PARAM_MAX);
	body->params[p_param] = p_value;
	body->param_set[p_param] = true;

	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	switch (p_param) {
		case PS3DE::BODY_PARAM_MASS:
		case PS3DE::BODY_PARAM_INERTIA:
		case PS3DE::BODY_PARAM_CENTER_OF_MASS: {
			_apply_body_mass(body);
		} break;
		case PS3DE::BODY_PARAM_FRICTION:
		case PS3DE::BODY_PARAM_BOUNCE: {
			const PxU32 count = body->actor ? body->actor->getNbShapes() : 0;
			PxShape **shapes = (PxShape **)alloca(sizeof(PxShape *) * MAX(count, (PxU32)1));
			if (body->actor) {
				body->actor->getShapes(shapes, count);
				for (PxU32 i = 0; i < count; i++) {
					PxMaterial *m = nullptr;
					shapes[i]->getMaterials(&m, 1);
					if (m) {
						m->setStaticFriction((float)body->params[PS3DE::BODY_PARAM_FRICTION]);
						m->setDynamicFriction((float)body->params[PS3DE::BODY_PARAM_FRICTION]);
						m->setRestitution((float)body->params[PS3DE::BODY_PARAM_BOUNCE]);
					}
				}
			}
		} break;
		case PS3DE::BODY_PARAM_LINEAR_DAMP: {
			if (dyn) {
				dyn->setLinearDamping((float)(real_t)p_value);
			}
		} break;
		case PS3DE::BODY_PARAM_ANGULAR_DAMP: {
			if (dyn) {
				dyn->setAngularDamping((float)(real_t)p_value);
			}
		} break;
		default: {
			// Gravity scale/damp modes handled at step() level (documented).
		} break;
	}
}

Variant PhysXPhysicsServer3D::body_get_param(RID p_body, PS3DE::BodyParameter p_param) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Variant());
	ERR_FAIL_INDEX_V((int)p_param, (int)PS3DE::BODY_PARAM_MAX, Variant());
	return body->params[p_param];
}

void PhysXPhysicsServer3D::body_reset_mass_properties(RID p_body) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	for (int i = 0; i < PS3DE::BODY_PARAM_MAX; i++) {
		body->param_set[i] = false;
	}
	body->params[PS3DE::BODY_PARAM_MASS] = 1.0;
	_apply_body_mass(body);
}

void PhysXPhysicsServer3D::body_set_state(RID p_body, PS3DE::BodyState p_state, const Variant &p_variant) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	switch (p_state) {
		case PS3DE::BODY_STATE_TRANSFORM: {
			if (body->actor) {
				body->actor->setGlobalPose(PhysXBackend::to_px((Transform3D)p_variant));
			}
		} break;
		case PS3DE::BODY_STATE_LINEAR_VELOCITY: {
			if (dyn) {
				dyn->setLinearVelocity(PhysXBackend::to_px((Vector3)p_variant));
			}
		} break;
		case PS3DE::BODY_STATE_ANGULAR_VELOCITY: {
			if (dyn) {
				dyn->setAngularVelocity(PhysXBackend::to_px((Vector3)p_variant));
			}
		} break;
		case PS3DE::BODY_STATE_SLEEPING: {
			if (dyn) {
				if ((bool)p_variant) {
					dyn->putToSleep();
				} else {
					dyn->wakeUp();
				}
			}
		} break;
		case PS3DE::BODY_STATE_CAN_SLEEP: {
			if (dyn) {
				dyn->setRigidBodyFlag(PxRigidBodyFlag::eENABLE_CCD, body->ccd);
				// eKINEMATIC kept; sleep toggle via sleep threshold below.
				if ((bool)p_variant) {
					dyn->setSleepThreshold(0.05f);
				} else {
					dyn->setSleepThreshold(0.0f);
				}
			}
		} break;
	}
}

Variant PhysXPhysicsServer3D::body_get_state(RID p_body, PS3DE::BodyState p_state) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Variant());
	const PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	switch (p_state) {
		case PS3DE::BODY_STATE_TRANSFORM: {
			return body->actor ? PhysXBackend::from_px(body->actor->getGlobalPose()) : Transform3D();
		} break;
		case PS3DE::BODY_STATE_LINEAR_VELOCITY: {
			return dyn ? PhysXBackend::from_px(dyn->getLinearVelocity()) : Vector3();
		} break;
		case PS3DE::BODY_STATE_ANGULAR_VELOCITY: {
			return dyn ? PhysXBackend::from_px(dyn->getAngularVelocity()) : Vector3();
		} break;
		case PS3DE::BODY_STATE_SLEEPING: {
			return dyn ? (bool)dyn->isSleeping() : true;
		} break;
		case PS3DE::BODY_STATE_CAN_SLEEP: {
			return dyn ? dyn->getSleepThreshold() > 0.0f : true;
		} break;
	}
	return Variant();
}

void PhysXPhysicsServer3D::body_apply_central_impulse(RID p_body, const Vector3 &p_impulse) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		dyn->addForce(PhysXBackend::to_px(p_impulse), PxForceMode::eIMPULSE);
	}
}

void PhysXPhysicsServer3D::body_apply_impulse(RID p_body, const Vector3 &p_impulse, const Vector3 &p_position) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		const PxTransform pose = dyn->getGlobalPose();
		PxRigidBodyExt::addForceAtPos(*dyn, PhysXBackend::to_px(p_impulse), pose.transform(PhysXBackend::to_px(p_position)), PxForceMode::eIMPULSE);
	}
}

void PhysXPhysicsServer3D::body_apply_torque_impulse(RID p_body, const Vector3 &p_impulse) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		dyn->addTorque(PhysXBackend::to_px(p_impulse), PxForceMode::eIMPULSE);
	}
}

void PhysXPhysicsServer3D::body_apply_central_force(RID p_body, const Vector3 &p_force) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		dyn->addForce(PhysXBackend::to_px(p_force), PxForceMode::eFORCE);
	}
}

void PhysXPhysicsServer3D::body_apply_force(RID p_body, const Vector3 &p_force, const Vector3 &p_position) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		const PxTransform pose = dyn->getGlobalPose();
		PxRigidBodyExt::addForceAtPos(*dyn, PhysXBackend::to_px(p_force), pose.transform(PhysXBackend::to_px(p_position)), PxForceMode::eFORCE);
	}
}

void PhysXPhysicsServer3D::body_apply_torque(RID p_body, const Vector3 &p_torque) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		dyn->addTorque(PhysXBackend::to_px(p_torque), PxForceMode::eFORCE);
	}
}

void PhysXPhysicsServer3D::body_add_constant_central_force(RID p_body, const Vector3 &p_force) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->constant_force += p_force;
}

void PhysXPhysicsServer3D::body_add_constant_force(RID p_body, const Vector3 &p_force, const Vector3 &p_position) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->constant_force += p_force;
}

void PhysXPhysicsServer3D::body_add_constant_torque(RID p_body, const Vector3 &p_torque) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->constant_torque += p_torque;
}

void PhysXPhysicsServer3D::body_set_constant_force(RID p_body, const Vector3 &p_force) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->constant_force = p_force;
}

Vector3 PhysXPhysicsServer3D::body_get_constant_force(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Vector3());
	return body->constant_force;
}

void PhysXPhysicsServer3D::body_set_constant_torque(RID p_body, const Vector3 &p_torque) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->constant_torque = p_torque;
}

Vector3 PhysXPhysicsServer3D::body_get_constant_torque(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Vector3());
	return body->constant_torque;
}

void PhysXPhysicsServer3D::body_set_axis_velocity(RID p_body, const Vector3 &p_axis_velocity) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (dyn) {
		const Vector3 axis = p_axis_velocity.normalized();
		const Vector3 v = dyn ? PhysXBackend::from_px(dyn->getLinearVelocity()) : Vector3();
		dyn->setLinearVelocity(PhysXBackend::to_px(v - axis * axis.dot(v) + p_axis_velocity));
	}
}

void PhysXPhysicsServer3D::body_set_axis_lock(RID p_body, PS3DE::BodyAxis p_axis, bool p_lock) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (!dyn) {
		return;
	}
	PxRigidDynamicLockFlags flags = dyn->getRigidDynamicLockFlags();
	PxRigidDynamicLockFlag::Enum px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_X;
	switch (p_axis) {
		case PS3DE::BODY_AXIS_LINEAR_X: px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_X; break;
		case PS3DE::BODY_AXIS_LINEAR_Y: px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_Y; break;
		case PS3DE::BODY_AXIS_LINEAR_Z: px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_Z; break;
		case PS3DE::BODY_AXIS_ANGULAR_X: px_flag = PxRigidDynamicLockFlag::eLOCK_ANGULAR_X; break;
		case PS3DE::BODY_AXIS_ANGULAR_Y: px_flag = PxRigidDynamicLockFlag::eLOCK_ANGULAR_Y; break;
		case PS3DE::BODY_AXIS_ANGULAR_Z: px_flag = PxRigidDynamicLockFlag::eLOCK_ANGULAR_Z; break;
	}
	if (p_lock) {
		flags |= px_flag;
	} else {
		flags &= ~px_flag;
	}
	dyn->setRigidDynamicLockFlags(flags);
}

bool PhysXPhysicsServer3D::body_is_axis_locked(RID p_body, PS3DE::BodyAxis p_axis) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);
	const PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
	if (!dyn) {
		return false;
	}
	PxRigidDynamicLockFlag::Enum px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_X;
	switch (p_axis) {
		case PS3DE::BODY_AXIS_LINEAR_X: px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_X; break;
		case PS3DE::BODY_AXIS_LINEAR_Y: px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_Y; break;
		case PS3DE::BODY_AXIS_LINEAR_Z: px_flag = PxRigidDynamicLockFlag::eLOCK_LINEAR_Z; break;
		case PS3DE::BODY_AXIS_ANGULAR_X: px_flag = PxRigidDynamicLockFlag::eLOCK_ANGULAR_X; break;
		case PS3DE::BODY_AXIS_ANGULAR_Y: px_flag = PxRigidDynamicLockFlag::eLOCK_ANGULAR_Y; break;
		case PS3DE::BODY_AXIS_ANGULAR_Z: px_flag = PxRigidDynamicLockFlag::eLOCK_ANGULAR_Z; break;
	}
	return dyn->getRigidDynamicLockFlags() & px_flag;
}

void PhysXPhysicsServer3D::body_add_collision_exception(RID p_body, RID p_body_b) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->exceptions.insert(p_body_b);
	PhysXBody *const *pp_b = body_map.getptr(p_body_b); PhysXBody *other = pp_b ? *pp_b : nullptr;
	if (other) {
		PhysXBackend::add_collision_exception(body, other);
	}
}

void PhysXPhysicsServer3D::body_remove_collision_exception(RID p_body, RID p_body_b) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->exceptions.erase(p_body_b);
	PhysXBody *const *pp_b = body_map.getptr(p_body_b); PhysXBody *other = pp_b ? *pp_b : nullptr;
	if (other) {
		PhysXBackend::remove_collision_exception(body, other);
	}
}

void PhysXPhysicsServer3D::body_get_collision_exceptions(RID p_body, List<RID> *p_exceptions) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	for (const RID &E : body->exceptions) {
		p_exceptions->push_back(E);
	}
}

void PhysXPhysicsServer3D::body_set_max_contacts_reported(RID p_body, int p_contacts) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->max_contacts_reported = p_contacts;
}

int PhysXPhysicsServer3D::body_get_max_contacts_reported(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return body->max_contacts_reported;
}

void PhysXPhysicsServer3D::body_set_contacts_reported_depth_threshold(RID p_body, real_t p_threshold) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->contacts_depth_threshold = p_threshold;
}

real_t PhysXPhysicsServer3D::body_get_contacts_reported_depth_threshold(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return body->contacts_depth_threshold;
}

void PhysXPhysicsServer3D::body_set_omit_force_integration(RID p_body, bool p_omit) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->omit_force_integration = p_omit;
}

bool PhysXPhysicsServer3D::body_is_omitting_force_integration(RID p_body) const {
	const PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);
	return body->omit_force_integration;
}

void PhysXPhysicsServer3D::body_set_state_sync_callback(RID p_body, const Callable &p_callable) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->state_sync_callback = p_callable;
}

void PhysXPhysicsServer3D::body_set_force_integration_callback(RID p_body, const Callable &p_callable, const Variant &p_udata) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->force_integration_callback = p_callable;
	body->force_integration_udata = p_udata;
}

void PhysXPhysicsServer3D::body_set_ray_pickable(RID p_body, bool p_enable) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->ray_pickable = p_enable;
}

PhysicsDirectBodyState3D *PhysXPhysicsServer3D::body_get_direct_state(RID p_body) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, nullptr);
	static PhysXDirectBodyState *state = nullptr;
	if (!state) {
		state = memnew(PhysXDirectBodyState);
	}
	state->body = body;
	return state;
}

bool PhysXPhysicsServer3D::body_test_motion(RID p_body, const PS3DT::MotionParameters &p_parameters, PS3DT::MotionResult *r_result) {
	PhysXBody *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);
	PhysXSpace *sp = space_owner.get_or_null(body->space);
	if (!sp || !sp->scene || !body->actor) {
		return false;
	}

	const Vector3 motion = p_parameters.motion;
	const real_t len = motion.length();
	if (len <= 0) {
		return false;
	}

	// Sweep the body's first shape geometry along the motion.
	PhysXShape *s = body->shapes.is_empty() ? nullptr : shape_owner.get_or_null(body->shapes[0].shape_rid);
	if (!s || !s->valid) {
		return false;
	}

	PhysXQueryFilter filter;
	filter.exclude.insert(body);
	for (const RID &E : p_parameters.exclude_bodies) {
		PhysXBody *other = body_owner.get_or_null(E);
		if (other) {
			filter.exclude.insert(other);
		}
	}

	PxSweepBuffer buffer;
	PxQueryFilterData qdata;
	qdata.flags = PxQueryFlag::eSTATIC | PxQueryFlag::eDYNAMIC | PxQueryFlag::ePREFILTER;
	const PxTransform pose = PhysXBackend::to_px(p_parameters.from);
	const bool hit = sp->scene->sweep(s->geometry.any(), pose, PhysXBackend::to_px(motion / len), len, buffer, PxHitFlag::eDEFAULT, qdata, &filter);

	if (!hit) {
		if (r_result) {
			r_result->travel = motion;
			r_result->remainder = Vector3();
			r_result->collision_safe_fraction = 1.0;
			r_result->collision_unsafe_fraction = 1.0;
		}
		return false;
	}

	const PxSweepHit &sweep = buffer.block;
	const real_t safe_fraction = (real_t)sweep.distance / len;
	if (r_result) {
		r_result->travel = motion * safe_fraction;
		r_result->remainder = motion - r_result->travel;
		r_result->collision_safe_fraction = safe_fraction;
		r_result->collision_unsafe_fraction = safe_fraction;
		r_result->collision_count = 1;
		PS3DT::MotionCollision &col = r_result->collisions[0];
		col.position = PhysXBackend::from_px(sweep.position);
		col.normal = PhysXBackend::from_px(sweep.normal);
		col.depth = 0;
		if (sweep.actor) {
			PhysXPhysicsServer3D::PhysXObject *owner = (PhysXPhysicsServer3D::PhysXObject *)sweep.actor->userData;
			if (owner) {
				col.collider = owner->rid;
				col.collider_id = owner->instance_id;
			}
		}
	}
	return true;
}

// ---- Soft bodies (unsupported) ----

RID PhysXPhysicsServer3D::soft_body_create() {
	WARN_PRINT_ONCE("PhysX Physics: soft bodies are not supported by the PhysX backend; the created soft body will not be simulated.");
	return RID();
}

// ---------------------------------------------------------------------------
// Joints.
// ---------------------------------------------------------------------------

RID PhysXPhysicsServer3D::joint_create() {
	const RID rid = joint_owner.make_rid();
	PhysXJoint *joint = joint_owner.get_or_null(rid);
	joint->rid = rid;
	return rid;
}

void PhysXPhysicsServer3D::joint_clear(RID p_joint) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	_destroy_joint(joint);
	joint->type = PS3DE::JOINT_TYPE_PIN;
}

PS3DE::JointType PhysXPhysicsServer3D::joint_get_type(RID p_joint) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, PS3DE::JOINT_TYPE_PIN);
	return joint->type;
}

void PhysXPhysicsServer3D::joint_set_solver_priority(RID p_joint, int p_priority) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->solver_priority = p_priority;
}

int PhysXPhysicsServer3D::joint_get_solver_priority(RID p_joint) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	return joint->solver_priority;
}

void PhysXPhysicsServer3D::joint_disable_collisions_between_bodies(RID p_joint, bool p_disable) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->disable_collisions = p_disable;
	if (joint->px_joint) {
		joint->px_joint->setConstraintFlag(PxConstraintFlag::eCOLLISION_ENABLED, !p_disable);
	}
}

bool PhysXPhysicsServer3D::joint_is_disabled_collisions_between_bodies(RID p_joint) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, true);
	return joint->disable_collisions;
}

void PhysXPhysicsServer3D::joint_make_pin(RID p_joint, RID p_body_A, const Vector3 &p_local_A, RID p_body_B, const Vector3 &p_local_B) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	_destroy_joint(joint);
	PhysXBody *ba = body_owner.get_or_null(p_body_A);
	PhysXBody *bb = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(ba);
	ERR_FAIL_NULL(bb);
	PxPhysics *px = backend->get_physics();
	ERR_FAIL_NULL(px);
	PxRigidActor *actor_a = ba->actor;
	PxRigidActor *actor_b = bb->actor;
	const PxTransform frame_a(PhysXBackend::to_px(p_local_A), PxQuat(PxIdentity));
	const PxTransform frame_b(PhysXBackend::to_px(p_local_B), PxQuat(PxIdentity));
	PxSphericalJoint *j = PxSphericalJointCreate(*px, actor_a, frame_a, actor_b, frame_b);
	ERR_FAIL_NULL(j);
	j->setConstraintFlag(PxConstraintFlag::eCOLLISION_ENABLED, !joint->disable_collisions);
	joint->px_joint = j;
	joint->type = PS3DE::JOINT_TYPE_PIN;
	joint->body_a = p_body_A;
	joint->body_b = p_body_B;
}

void PhysXPhysicsServer3D::pin_joint_set_param(RID p_joint, PS3DE::PinJointParam p_param, real_t p_value) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->pin_params[p_param] = p_value;
}

real_t PhysXPhysicsServer3D::pin_joint_get_param(RID p_joint, PS3DE::PinJointParam p_param) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	const HashMap<int, real_t>::ConstIterator it = joint->pin_params.find(p_param);
	return it ? it->value : 0;
}

void PhysXPhysicsServer3D::pin_joint_set_local_a(RID p_joint, const Vector3 &p_A) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	PxSphericalJoint *j = joint->px_joint ? joint->px_joint->is<PxSphericalJoint>() : nullptr;
	if (j) {
		j->setLocalPose(PxJointActorIndex::eACTOR0, PxTransform(PhysXBackend::to_px(p_A), PxQuat(PxIdentity)));
	}
}

Vector3 PhysXPhysicsServer3D::pin_joint_get_local_a(RID p_joint) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, Vector3());
	const PxSphericalJoint *j = joint->px_joint ? joint->px_joint->is<PxSphericalJoint>() : nullptr;
	return j ? PhysXBackend::from_px(j->getLocalPose(PxJointActorIndex::eACTOR0).p) : Vector3();
}

void PhysXPhysicsServer3D::pin_joint_set_local_b(RID p_joint, const Vector3 &p_B) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	PxSphericalJoint *j = joint->px_joint ? joint->px_joint->is<PxSphericalJoint>() : nullptr;
	if (j) {
		j->setLocalPose(PxJointActorIndex::eACTOR1, PxTransform(PhysXBackend::to_px(p_B), PxQuat(PxIdentity)));
	}
}

Vector3 PhysXPhysicsServer3D::pin_joint_get_local_b(RID p_joint) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, Vector3());
	const PxSphericalJoint *j = joint->px_joint ? joint->px_joint->is<PxSphericalJoint>() : nullptr;
	return j ? PhysXBackend::from_px(j->getLocalPose(PxJointActorIndex::eACTOR1).p) : Vector3();
}

void PhysXPhysicsServer3D::joint_make_hinge(RID p_joint, RID p_body_A, const Transform3D &p_hinge_A, RID p_body_B, const Transform3D &p_hinge_B) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	_destroy_joint(joint);
	PhysXBody *ba = body_owner.get_or_null(p_body_A);
	PhysXBody *bb = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(ba);
	ERR_FAIL_NULL(bb);
	PxPhysics *px = backend->get_physics();
	ERR_FAIL_NULL(px);
	PxRevoluteJoint *j = PxRevoluteJointCreate(*px, ba->actor, PhysXBackend::to_px(p_hinge_A), bb->actor, PhysXBackend::to_px(p_hinge_B));
	ERR_FAIL_NULL(j);
	j->setConstraintFlag(PxConstraintFlag::eCOLLISION_ENABLED, !joint->disable_collisions);
	joint->px_joint = j;
	joint->type = PS3DE::JOINT_TYPE_HINGE;
	joint->body_a = p_body_A;
	joint->body_b = p_body_B;
}

void PhysXPhysicsServer3D::joint_make_hinge_simple(RID p_joint, RID p_body_A, const Vector3 &p_pivot_A, const Vector3 &p_axis_A, RID p_body_B, const Vector3 &p_pivot_B, const Vector3 &p_axis_B) {
	Transform3D ha;
	ha.basis = Basis(Quaternion(Vector3(1, 0, 0), p_axis_A));
	ha.origin = p_pivot_A;
	Transform3D hb;
	hb.basis = Basis(Quaternion(Vector3(1, 0, 0), p_axis_B));
	hb.origin = p_pivot_B;
	joint_make_hinge(p_joint, p_body_A, ha, p_body_B, hb);
}

void PhysXPhysicsServer3D::hinge_joint_set_param(RID p_joint, PS3DE::HingeJointParam p_param, real_t p_value) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->hinge_params[p_param] = p_value;
	PxRevoluteJoint *j = joint->px_joint ? joint->px_joint->is<PxRevoluteJoint>() : nullptr;
	if (!j) {
		return;
	}
	switch (p_param) {
		case PS3DE::HINGE_JOINT_LIMIT_UPPER: {
			const HashMap<int, real_t>::ConstIterator it = joint->hinge_params.find(PS3DE::HINGE_JOINT_LIMIT_LOWER);
			const real_t lower = it ? it->value : -Math::PI;
			j->setLimit(PxJointAngularLimitPair((float)lower, (float)p_value));
		} break;
		case PS3DE::HINGE_JOINT_LIMIT_LOWER: {
			const HashMap<int, real_t>::ConstIterator it = joint->hinge_params.find(PS3DE::HINGE_JOINT_LIMIT_UPPER);
			const real_t upper = it ? it->value : Math::PI;
			j->setLimit(PxJointAngularLimitPair((float)p_value, (float)upper));
		} break;
		case PS3DE::HINGE_JOINT_MOTOR_TARGET_VELOCITY: {
			j->setDriveVelocity((float)p_value);
		} break;
		case PS3DE::HINGE_JOINT_MOTOR_MAX_IMPULSE: {
			j->setDriveForceLimit((float)p_value * 60.0f); // Impulse -> force approximation.
		} break;
		default: {
			// Softness-style params cached only.
		} break;
	}
}

real_t PhysXPhysicsServer3D::hinge_joint_get_param(RID p_joint, PS3DE::HingeJointParam p_param) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	const HashMap<int, real_t>::ConstIterator it = joint->hinge_params.find(p_param);
	return it ? it->value : 0;
}

void PhysXPhysicsServer3D::hinge_joint_set_flag(RID p_joint, PS3DE::HingeJointFlag p_flag, bool p_enabled) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	PxRevoluteJoint *j = joint->px_joint ? joint->px_joint->is<PxRevoluteJoint>() : nullptr;
	if (!j) {
		return;
	}
	switch (p_flag) {
		case PS3DE::HINGE_JOINT_FLAG_USE_LIMIT: {
			j->setRevoluteJointFlag(PxRevoluteJointFlag::eLIMIT_ENABLED, p_enabled);
		} break;
		case PS3DE::HINGE_JOINT_FLAG_ENABLE_MOTOR: {
			j->setRevoluteJointFlag(PxRevoluteJointFlag::eDRIVE_ENABLED, p_enabled);
		} break;
		default: {
		} break;
	}
}

bool PhysXPhysicsServer3D::hinge_joint_get_flag(RID p_joint, PS3DE::HingeJointFlag p_flag) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, false);
	const PxRevoluteJoint *j = joint->px_joint ? joint->px_joint->is<PxRevoluteJoint>() : nullptr;
	if (!j) {
		return false;
	}
	switch (p_flag) {
		case PS3DE::HINGE_JOINT_FLAG_USE_LIMIT: {
			return j->getRevoluteJointFlags() & PxRevoluteJointFlag::eLIMIT_ENABLED;
		} break;
		case PS3DE::HINGE_JOINT_FLAG_ENABLE_MOTOR: {
			return j->getRevoluteJointFlags() & PxRevoluteJointFlag::eDRIVE_ENABLED;
		} break;
		default: {
			return false;
		}
	}
}

void PhysXPhysicsServer3D::joint_make_slider(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	_destroy_joint(joint);
	PhysXBody *ba = body_owner.get_or_null(p_body_A);
	PhysXBody *bb = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(ba);
	ERR_FAIL_NULL(bb);
	PxPhysics *px = backend->get_physics();
	ERR_FAIL_NULL(px);
	PxPrismaticJoint *j = PxPrismaticJointCreate(*px, ba->actor, PhysXBackend::to_px(p_local_frame_A), bb->actor, PhysXBackend::to_px(p_local_frame_B));
	ERR_FAIL_NULL(j);
	j->setConstraintFlag(PxConstraintFlag::eCOLLISION_ENABLED, !joint->disable_collisions);
	joint->px_joint = j;
	joint->type = PS3DE::JOINT_TYPE_SLIDER;
	joint->body_a = p_body_A;
	joint->body_b = p_body_B;
}

void PhysXPhysicsServer3D::slider_joint_set_param(RID p_joint, PS3DE::SliderJointParam p_param, real_t p_value) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->slider_params[p_param] = p_value;
	PxPrismaticJoint *j = joint->px_joint ? joint->px_joint->is<PxPrismaticJoint>() : nullptr;
	if (!j) {
		return;
	}
	if (p_param == PS3DE::SLIDER_JOINT_LINEAR_LIMIT_UPPER) {
		const HashMap<int, real_t>::ConstIterator it = joint->slider_params.find(PS3DE::SLIDER_JOINT_LINEAR_LIMIT_LOWER);
		const real_t lower = it ? it->value : -1.0;
		j->setLimit(PxJointLinearLimitPair(PxTolerancesScale(), (float)lower, (float)p_value));
	} else if (p_param == PS3DE::SLIDER_JOINT_LINEAR_LIMIT_LOWER) {
		const HashMap<int, real_t>::ConstIterator it = joint->slider_params.find(PS3DE::SLIDER_JOINT_LINEAR_LIMIT_UPPER);
		const real_t upper = it ? it->value : 1.0;
		j->setLimit(PxJointLinearLimitPair(PxTolerancesScale(), (float)p_value, (float)upper));
	}
}

real_t PhysXPhysicsServer3D::slider_joint_get_param(RID p_joint, PS3DE::SliderJointParam p_param) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	const HashMap<int, real_t>::ConstIterator it = joint->slider_params.find(p_param);
	return it ? it->value : 0;
}

void PhysXPhysicsServer3D::joint_make_cone_twist(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	_destroy_joint(joint);
	PhysXBody *ba = body_owner.get_or_null(p_body_A);
	PhysXBody *bb = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(ba);
	ERR_FAIL_NULL(bb);
	PxPhysics *px = backend->get_physics();
	ERR_FAIL_NULL(px);
	// Cone twist = D6 joint with swing/twist limits.
	PxD6Joint *j = PxD6JointCreate(*px, ba->actor, PhysXBackend::to_px(p_local_frame_A), bb->actor, PhysXBackend::to_px(p_local_frame_B));
	ERR_FAIL_NULL(j);
	j->setMotion(PxD6Axis::eSWING1, PxD6Motion::eLIMITED);
	j->setMotion(PxD6Axis::eSWING2, PxD6Motion::eLIMITED);
	j->setMotion(PxD6Axis::eTWIST, PxD6Motion::eLIMITED);
	j->setConstraintFlag(PxConstraintFlag::eCOLLISION_ENABLED, !joint->disable_collisions);
	joint->px_joint = j;
	joint->type = PS3DE::JOINT_TYPE_CONE_TWIST;
	joint->body_a = p_body_A;
	joint->body_b = p_body_B;
}

void PhysXPhysicsServer3D::cone_twist_joint_set_param(RID p_joint, PS3DE::ConeTwistJointParam p_param, real_t p_value) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->cone_twist_params[p_param] = p_value;
	PxD6Joint *j = joint->px_joint ? joint->px_joint->is<PxD6Joint>() : nullptr;
	if (!j || joint->type != PS3DE::JOINT_TYPE_CONE_TWIST) {
		return;
	}
	switch (p_param) {
		case PS3DE::CONE_TWIST_JOINT_SWING_SPAN: {
			j->setSwingLimit(PxJointLimitCone((float)p_value, (float)p_value));
		} break;
		case PS3DE::CONE_TWIST_JOINT_TWIST_SPAN: {
			j->setTwistLimit(PxJointAngularLimitPair((float)-p_value, (float)p_value));
		} break;
		default: {
			// Softness-style params cached only.
		} break;
	}
}

real_t PhysXPhysicsServer3D::cone_twist_joint_get_param(RID p_joint, PS3DE::ConeTwistJointParam p_param) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	const HashMap<int, real_t>::ConstIterator it = joint->cone_twist_params.find(p_param);
	return it ? it->value : 0;
}

void PhysXPhysicsServer3D::joint_make_generic_6dof(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	_destroy_joint(joint);
	PhysXBody *ba = body_owner.get_or_null(p_body_A);
	PhysXBody *bb = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(ba);
	ERR_FAIL_NULL(bb);
	PxPhysics *px = backend->get_physics();
	ERR_FAIL_NULL(px);
	PxD6Joint *j = PxD6JointCreate(*px, ba->actor, PhysXBackend::to_px(p_local_frame_A), bb->actor, PhysXBackend::to_px(p_local_frame_B));
	ERR_FAIL_NULL(j);
	j->setConstraintFlag(PxConstraintFlag::eCOLLISION_ENABLED, !joint->disable_collisions);
	joint->px_joint = j;
	joint->type = PS3DE::JOINT_TYPE_6DOF;
	joint->body_a = p_body_A;
	joint->body_b = p_body_B;
}

void PhysXPhysicsServer3D::generic_6dof_joint_set_param(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisParam p_param, real_t p_value) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->g6dof_params[(int)p_axis * 100 + (int)p_param] = p_value;
	PxD6Joint *j = joint->px_joint ? joint->px_joint->is<PxD6Joint>() : nullptr;
	if (!j || joint->type != PS3DE::JOINT_TYPE_6DOF) {
		return;
	}
	const PxD6Axis::Enum linear_axis = p_axis == Vector3::AXIS_X ? PxD6Axis::eX : (p_axis == Vector3::AXIS_Y ? PxD6Axis::eY : PxD6Axis::eZ);
	const PxD6Axis::Enum angular_axis = p_axis == Vector3::AXIS_X ? PxD6Axis::eTWIST : (p_axis == Vector3::AXIS_Y ? PxD6Axis::eSWING1 : PxD6Axis::eSWING2);

	switch (p_param) {
		case PS3DE::G6DOF_JOINT_LINEAR_LOWER_LIMIT:
		case PS3DE::G6DOF_JOINT_LINEAR_UPPER_LIMIT: {
			const int other_key = (int)p_axis * 100 + (p_param == PS3DE::G6DOF_JOINT_LINEAR_LOWER_LIMIT ? (int)PS3DE::G6DOF_JOINT_LINEAR_UPPER_LIMIT : (int)PS3DE::G6DOF_JOINT_LINEAR_LOWER_LIMIT);
			const HashMap<int, real_t>::ConstIterator it = joint->g6dof_params.find(other_key);
			const real_t other = it ? it->value : (p_param == PS3DE::G6DOF_JOINT_LINEAR_LOWER_LIMIT ? -1.0 : 1.0);
			const real_t lower = p_param == PS3DE::G6DOF_JOINT_LINEAR_LOWER_LIMIT ? p_value : other;
			const real_t upper = p_param == PS3DE::G6DOF_JOINT_LINEAR_UPPER_LIMIT ? p_value : other;
			j->setLinearLimit(linear_axis, PxJointLinearLimitPair(PxTolerancesScale(), (float)lower, (float)upper));
			j->setMotion(linear_axis, PxD6Motion::eLIMITED);
		} break;
		case PS3DE::G6DOF_JOINT_ANGULAR_LOWER_LIMIT:
		case PS3DE::G6DOF_JOINT_ANGULAR_UPPER_LIMIT: {
			const int other_key = (int)p_axis * 100 + (p_param == PS3DE::G6DOF_JOINT_ANGULAR_LOWER_LIMIT ? (int)PS3DE::G6DOF_JOINT_ANGULAR_UPPER_LIMIT : (int)PS3DE::G6DOF_JOINT_ANGULAR_LOWER_LIMIT);
			const HashMap<int, real_t>::ConstIterator it = joint->g6dof_params.find(other_key);
			const real_t other = it ? it->value : (p_param == PS3DE::G6DOF_JOINT_ANGULAR_LOWER_LIMIT ? -Math::PI : Math::PI);
			const real_t lower = p_param == PS3DE::G6DOF_JOINT_ANGULAR_LOWER_LIMIT ? p_value : other;
			const real_t upper = p_param == PS3DE::G6DOF_JOINT_ANGULAR_UPPER_LIMIT ? p_value : other;
			j->setTwistLimit(PxJointAngularLimitPair((float)lower, (float)upper));
			j->setMotion(angular_axis, PxD6Motion::eLIMITED);
		} break;
		case PS3DE::G6DOF_JOINT_LINEAR_MOTOR_TARGET_VELOCITY: {
			PxVec3 linear(0, 0, 0);
			if (p_axis == Vector3::AXIS_X) {
				linear.x = (float)p_value;
			} else if (p_axis == Vector3::AXIS_Y) {
				linear.y = (float)p_value;
			} else {
				linear.z = (float)p_value;
			}
			j->setDriveVelocity(linear, PxVec3(0, 0, 0));
		} break;
		case PS3DE::G6DOF_JOINT_ANGULAR_MOTOR_TARGET_VELOCITY: {
			PxVec3 angular(0, 0, 0);
			if (p_axis == Vector3::AXIS_X) {
				angular.x = (float)p_value;
			} else if (p_axis == Vector3::AXIS_Y) {
				angular.y = (float)p_value;
			} else {
				angular.z = (float)p_value;
			}
			j->setDriveVelocity(PxVec3(0, 0, 0), angular);
		} break;
		default: {
			// Spring/damping params cached (D6 drives approximate them).
		} break;
	}
}

real_t PhysXPhysicsServer3D::generic_6dof_joint_get_param(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisParam p_param) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	const HashMap<int, real_t>::ConstIterator it = joint->g6dof_params.find((int)p_axis * 100 + (int)p_param);
	return it ? it->value : 0;
}

void PhysXPhysicsServer3D::generic_6dof_joint_set_flag(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisFlag p_flag, bool p_enable) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->g6dof_flags[(int)p_axis * 100 + (int)p_flag] = p_enable;
	PxD6Joint *j = joint->px_joint ? joint->px_joint->is<PxD6Joint>() : nullptr;
	if (!j || joint->type != PS3DE::JOINT_TYPE_6DOF) {
		return;
	}
	const PxD6Axis::Enum linear_axis = p_axis == Vector3::AXIS_X ? PxD6Axis::eX : (p_axis == Vector3::AXIS_Y ? PxD6Axis::eY : PxD6Axis::eZ);
	const PxD6Axis::Enum angular_axis = p_axis == Vector3::AXIS_X ? PxD6Axis::eTWIST : (p_axis == Vector3::AXIS_Y ? PxD6Axis::eSWING1 : PxD6Axis::eSWING2);
	switch (p_flag) {
		case PS3DE::G6DOF_JOINT_FLAG_ENABLE_LINEAR_LIMIT: {
			j->setMotion(linear_axis, p_enable ? PxD6Motion::eLIMITED : PxD6Motion::eFREE);
		} break;
		case PS3DE::G6DOF_JOINT_FLAG_ENABLE_ANGULAR_LIMIT: {
			j->setMotion(angular_axis, p_enable ? PxD6Motion::eLIMITED : PxD6Motion::eFREE);
		} break;
		case PS3DE::G6DOF_JOINT_FLAG_ENABLE_LINEAR_MOTOR: {
			j->setDrive(linear_axis == PxD6Axis::eX ? PxD6Drive::eX : (linear_axis == PxD6Axis::eY ? PxD6Drive::eY : PxD6Drive::eZ), PxD6JointDrive(0, 0, FLT_MAX, p_enable));
		} break;
		case PS3DE::G6DOF_JOINT_FLAG_ENABLE_ANGULAR_MOTOR: {
			j->setDrive(angular_axis == PxD6Axis::eTWIST ? PxD6Drive::eTWIST : (angular_axis == PxD6Axis::eSWING1 ? PxD6Drive::eSWING1 : PxD6Drive::eSWING2), PxD6JointDrive(0, 0, FLT_MAX, p_enable));
		} break;
		default: {
			// Spring flags cached.
		} break;
	}
}

bool PhysXPhysicsServer3D::generic_6dof_joint_get_flag(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisFlag p_flag) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, false);
	const HashMap<int, bool>::ConstIterator it = joint->g6dof_flags.find((int)p_axis * 100 + (int)p_flag);
	return it ? it->value : false;
}

void PhysXPhysicsServer3D::generic_6dof_joint_set_angular_target_rotation(RID p_joint, const Quaternion &p_target_rotation) {
	PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->g6dof_target_rotation = p_target_rotation;
}

Quaternion PhysXPhysicsServer3D::generic_6dof_joint_get_angular_target_rotation(RID p_joint) const {
	const PhysXJoint *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, Quaternion());
	return joint->g6dof_target_rotation;
}

// ---------------------------------------------------------------------------
// Lifecycle.
// ---------------------------------------------------------------------------

void PhysXPhysicsServer3D::free_rid(RID p_rid) {
	if (shape_owner.owns(p_rid)) {
		// Shapes still attached to bodies/areas are detached lazily (px_shape
		// pointers become stale only on rebuild; geometry releases at free).
		shape_owner.free(p_rid);
		return;
	}
	if (body_owner.owns(p_rid)) {
		PhysXBody *body = body_owner.get_or_null(p_rid);
		for (int i = body->shapes.size() - 1; i >= 0; i--) {
			_body_remove_px_shape(body, i);
		}
		PhysXSpace *sp = space_owner.get_or_null(body->space);
		if (sp && sp->scene && body->actor) {
			sp->scene->removeActor(*body->actor);
		}
		if (body->actor) {
			body->actor->release();
		}
		body_map.erase(p_rid);
		body_owner.free(p_rid);
		return;
	}
	if (area_owner.owns(p_rid)) {
		PhysXArea *area = area_owner.get_or_null(p_rid);
		for (int i = area->shapes.size() - 1; i >= 0; i--) {
			_area_remove_px_shape(area, i);
		}
		PhysXSpace *sp = space_owner.get_or_null(area->space);
		if (sp && sp->scene && area->actor) {
			sp->scene->removeActor(*area->actor);
		}
		if (area->actor) {
			area->actor->release();
		}
		area_map.erase(p_rid);
		area_owner.free(p_rid);
		return;
	}
	if (joint_owner.owns(p_rid)) {
		PhysXJoint *joint = joint_owner.get_or_null(p_rid);
		_destroy_joint(joint);
		joint_owner.free(p_rid);
		return;
	}
	if (space_owner.owns(p_rid)) {
		PhysXSpace *sp = space_owner.get_or_null(p_rid);
		if (direct_space_states.has(p_rid)) {
			memdelete(direct_space_states[p_rid]);
			direct_space_states.erase(p_rid);
		}
		if (sp->scene) {
			backend->destroy_scene(sp->scene);
			sp->scene = nullptr;
		}
		if (default_space == p_rid) {
			default_space = RID();
		}
		space_owner.free(p_rid);
		return;
	}
}

void PhysXPhysicsServer3D::init() {
	// Apply the physics/physx_physics_3d/* project settings before startup.
	PhysXBackend::Config &cfg = backend->get_config();
	ProjectSettings *ps = ProjectSettings::get_singleton();
	if (ps) {
		auto get_bool = [&](const char *p_name, bool p_current) -> bool {
			return ps->has_setting(p_name) ? (bool)ps->get_setting(p_name) : p_current;
		};
		cfg.gpu_acceleration = get_bool("physics/physx_physics_3d/gpu_acceleration", cfg.gpu_acceleration);
		cfg.cooking = get_bool("physics/physx_physics_3d/cooking", cfg.cooking);
		cfg.character_controller = get_bool("physics/physx_physics_3d/character_controller", cfg.character_controller);
		cfg.vehicle = get_bool("physics/physx_physics_3d/vehicle", cfg.vehicle);
		cfg.broadphase_mbp = get_bool("physics/physx_physics_3d/broadphase_mbp", cfg.broadphase_mbp);
		if (ps->has_setting("physics/physx_physics_3d/simulation_step")) {
			cfg.simulation_step = (real_t)ps->get_setting("physics/physx_physics_3d/simulation_step");
		}
		if (ps->has_setting("physics/physx_physics_3d/max_sub_steps")) {
			cfg.max_sub_steps = (int)ps->get_setting("physics/physx_physics_3d/max_sub_steps");
		}
	}
	backend->init();
}

void PhysXPhysicsServer3D::step(real_t p_step) {
	if (!backend->get_physics()) {
		return;
	}

	// Apply constant forces and force-integration callbacks before simulating.
	for (const KeyValue<RID, PhysXBody *> &E : body_map) {
		PhysXBody *body = E.value;
		PxRigidDynamic *dyn = body->actor ? body->actor->is<PxRigidDynamic>() : nullptr;
		if (!dyn || body->mode == PS3DE::BODY_MODE_KINEMATIC) {
			continue;
		}
		if (!body->constant_force.is_zero_approx()) {
			dyn->addForce(PhysXBackend::to_px(body->constant_force), PxForceMode::eFORCE);
		}
		if (!body->constant_torque.is_zero_approx()) {
			dyn->addTorque(PhysXBackend::to_px(body->constant_torque), PxForceMode::eFORCE);
		}
	}

	backend->simulate((float)p_step);
	backend->fetch_results(true);

	// State sync + force integration callbacks after the step.
	for (const KeyValue<RID, PhysXBody *> &E : body_map) {
		PhysXBody *body = E.value;
		if (body->state_sync_callback.is_valid()) {
			body->state_sync_callback.call(body->rid, body->object_id);
		}
		if (body->force_integration_callback.is_valid() && !body->omit_force_integration) {
			body->force_integration_callback.call(body_get_direct_state(body->rid), body->force_integration_udata);
		}
	}

	_check_area_overlaps();
}

void PhysXPhysicsServer3D::sync() {
}

void PhysXPhysicsServer3D::flush_queries() {
}

void PhysXPhysicsServer3D::end_sync() {
}

void PhysXPhysicsServer3D::finish() {
	// Free all resources; RID owners destruct their entries.
	Vector<RID> all_bodies;
	for (const KeyValue<RID, PhysXBody *> &E : body_map) {
		all_bodies.push_back(E.key);
	}
	for (int i = 0; i < all_bodies.size(); i++) {
		free_rid(all_bodies[i]);
	}
	Vector<RID> all_areas;
	for (const KeyValue<RID, PhysXArea *> &E : area_map) {
		all_areas.push_back(E.key);
	}
	for (int i = 0; i < all_areas.size(); i++) {
		free_rid(all_areas[i]);
	}
	Vector<RID> all_spaces;
	// Collect space RIDs from direct state map + owner (spaces tracked by iterating owner is not exposed; track via scenes list).
	for (const KeyValue<RID, PhysXDirectSpaceState *> &E : direct_space_states) {
		all_spaces.push_back(E.key);
	}
	for (int i = 0; i < all_spaces.size(); i++) {
		free_rid(all_spaces[i]);
	}
	if (backend.is_valid()) {
		backend->shutdown();
	}
}
