/**************************************************************************/
/*  kaizen_physics_server_3d.cpp                                          */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             KAIZEN ENGINE                              */
/*                        https://godotengine.org                         */
/**************************************************************************/
/* Copyright (c) 2014-present Kaizen Engine contributors (see AUTHORS.md). */
/* Copyright (c) 2007-2014 Juan Linietsky, Ariel Manzur.                  */
/*                                                                        */
/* Permission is hereby granted, free of charge, to any person obtaining  */
/* a copy of this software and associated documentation files (the        */
/* "Software"), to deal in the Software without restriction, including    */
/* without limitation the rights to use, copy, modify, merge, publish,    */
/* distribute, sublicense, and/or sell copies of the Software, and to     */
/* permit persons to whom the Software is furnished to do so, subject to  */
/* the following conditions:                                              */
/*                                                                        */
/* The above copyright notice and this permission notice shall be         */
/* included in all copies or substantial portions of the Software.        */
/*                                                                        */
/* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,        */
/* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF     */
/* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. */
/* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY   */
/* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,   */
/* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE      */
/* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                 */
/**************************************************************************/

#include "kaizen_physics_server_3d.h"

#include "kaizen_body_direct_state_3d.h"
#include "kaizen_broad_phase_3d_bvh.h"
#include "joints/kaizen_cone_twist_joint_3d.h"
#include "joints/kaizen_generic_6dof_joint_3d.h"
#include "joints/kaizen_hinge_joint_3d.h"
#include "joints/kaizen_pin_joint_3d.h"
#include "joints/kaizen_slider_joint_3d.h"

#include "core/debugger/engine_debugger.h"
#include "core/os/os.h"

#define FLUSH_QUERY_CHECK(m_object) \
	ERR_FAIL_COND_MSG(m_object->get_space() && flushing_queries, "Can't change this state while flushing queries. Use call_deferred() or set_deferred() to change monitoring state instead.");

RID KaizenPhysicsServer3D::world_boundary_shape_create() {
	KaizenShape3D *shape = memnew(KaizenWorldBoundaryShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::separation_ray_shape_create() {
	KaizenShape3D *shape = memnew(KaizenSeparationRayShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::sphere_shape_create() {
	KaizenShape3D *shape = memnew(KaizenSphereShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::box_shape_create() {
	KaizenShape3D *shape = memnew(KaizenBoxShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::capsule_shape_create() {
	KaizenShape3D *shape = memnew(KaizenCapsuleShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::cylinder_shape_create() {
	KaizenShape3D *shape = memnew(KaizenCylinderShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::convex_polygon_shape_create() {
	KaizenShape3D *shape = memnew(KaizenConvexPolygonShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::concave_polygon_shape_create() {
	KaizenShape3D *shape = memnew(KaizenConcavePolygonShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::heightmap_shape_create() {
	KaizenShape3D *shape = memnew(KaizenHeightMapShape3D);
	RID rid = shape_owner.make_rid(shape);
	shape->set_self(rid);
	return rid;
}
RID KaizenPhysicsServer3D::custom_shape_create() {
	ERR_FAIL_V(RID());
}

void KaizenPhysicsServer3D::shape_set_data(RID p_shape, const Variant &p_data) {
	KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(shape);
	shape->set_data(p_data);
}

void KaizenPhysicsServer3D::shape_set_custom_solver_bias(RID p_shape, real_t p_bias) {
	KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(shape);
	shape->set_custom_bias(p_bias);
}

PS3DE::ShapeType KaizenPhysicsServer3D::shape_get_type(RID p_shape) const {
	const KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL_V(shape, PS3DE::SHAPE_CUSTOM);
	return shape->get_type();
}

Variant KaizenPhysicsServer3D::shape_get_data(RID p_shape) const {
	const KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL_V(shape, Variant());
	ERR_FAIL_COND_V(!shape->is_configured(), Variant());
	return shape->get_data();
}

void KaizenPhysicsServer3D::shape_set_margin(RID p_shape, real_t p_margin) {
}

real_t KaizenPhysicsServer3D::shape_get_margin(RID p_shape) const {
	return 0.0;
}

real_t KaizenPhysicsServer3D::shape_get_custom_solver_bias(RID p_shape) const {
	const KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL_V(shape, 0);
	return shape->get_custom_bias();
}

RID KaizenPhysicsServer3D::space_create() {
	KaizenSpace3D *space = memnew(KaizenSpace3D);
	RID id = space_owner.make_rid(space);
	space->set_self(id);
	RID area_id = area_create();
	KaizenArea3D *area = area_owner.get_or_null(area_id);
	ERR_FAIL_NULL_V(area, RID());
	space->set_default_area(area);
	area->set_space(space);
	area->set_priority(-1);
	RID sgb = body_create();
	body_set_space(sgb, id);
	body_set_mode(sgb, PS3DE::BODY_MODE_STATIC);
	space->set_static_global_body(sgb);

	return id;
}

void KaizenPhysicsServer3D::space_set_active(RID p_space, bool p_active) {
	KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL(space);
	if (p_active) {
		active_spaces.insert(space);
	} else {
		active_spaces.erase(space);
	}
}

bool KaizenPhysicsServer3D::space_is_active(RID p_space) const {
	KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(space, false);

	return active_spaces.has(space);
}

void KaizenPhysicsServer3D::space_set_param(RID p_space, PS3DE::SpaceParameter p_param, real_t p_value) {
	KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL(space);

	space->set_param(p_param, p_value);
}

real_t KaizenPhysicsServer3D::space_get_param(RID p_space, PS3DE::SpaceParameter p_param) const {
	const KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(space, 0);
	return space->get_param(p_param);
}

PhysicsDirectSpaceState3D *KaizenPhysicsServer3D::space_get_direct_state(RID p_space) {
	KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(space, nullptr);
	ERR_FAIL_COND_V_MSG((using_threads && !doing_sync) || space->is_locked(), nullptr, "Space state is inaccessible right now, wait for iteration or physics process notification.");

	return space->get_direct_state();
}

void KaizenPhysicsServer3D::space_set_debug_contacts(RID p_space, int p_max_contacts) {
	KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL(space);
	space->set_debug_contacts(p_max_contacts);
}

Vector<Vector3> KaizenPhysicsServer3D::space_get_contacts(RID p_space) const {
	KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(space, Vector<Vector3>());
	return space->get_debug_contacts();
}

int KaizenPhysicsServer3D::space_get_contact_count(RID p_space) const {
	KaizenSpace3D *space = space_owner.get_or_null(p_space);
	ERR_FAIL_NULL_V(space, 0);
	return space->get_debug_contact_count();
}

RID KaizenPhysicsServer3D::area_create() {
	KaizenArea3D *area = memnew(KaizenArea3D);
	RID rid = area_owner.make_rid(area);
	area->set_self(rid);
	return rid;
}

void KaizenPhysicsServer3D::area_set_space(RID p_area, RID p_space) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	KaizenSpace3D *space = nullptr;
	if (p_space.is_valid()) {
		space = space_owner.get_or_null(p_space);
		ERR_FAIL_NULL(space);
	}

	if (area->get_space() == space) {
		return; //pointless
	}

	area->clear_constraints();
	area->set_space(space);
}

RID KaizenPhysicsServer3D::area_get_space(RID p_area) const {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, RID());

	KaizenSpace3D *space = area->get_space();
	if (!space) {
		return RID();
	}
	return space->get_self();
}

void KaizenPhysicsServer3D::area_add_shape(RID p_area, RID p_shape, const Transform3D &p_transform, bool p_disabled) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(shape);

	area->add_shape(shape, p_transform, p_disabled);
}

void KaizenPhysicsServer3D::area_set_shape(RID p_area, int p_shape_idx, RID p_shape) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(shape);
	ERR_FAIL_COND(!shape->is_configured());

	area->set_shape(p_shape_idx, shape);
}

void KaizenPhysicsServer3D::area_set_shape_transform(RID p_area, int p_shape_idx, const Transform3D &p_transform) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	area->set_shape_transform(p_shape_idx, p_transform);
}

int KaizenPhysicsServer3D::area_get_shape_count(RID p_area) const {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, -1);

	return area->get_shape_count();
}

RID KaizenPhysicsServer3D::area_get_shape(RID p_area, int p_shape_idx) const {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, RID());

	KaizenShape3D *shape = area->get_shape(p_shape_idx);
	ERR_FAIL_NULL_V(shape, RID());

	return shape->get_self();
}

Transform3D KaizenPhysicsServer3D::area_get_shape_transform(RID p_area, int p_shape_idx) const {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, Transform3D());

	return area->get_shape_transform(p_shape_idx);
}

void KaizenPhysicsServer3D::area_remove_shape(RID p_area, int p_shape_idx) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	area->remove_shape(p_shape_idx);
}

void KaizenPhysicsServer3D::area_clear_shapes(RID p_area) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	while (area->get_shape_count()) {
		area->remove_shape(0);
	}
}

void KaizenPhysicsServer3D::area_set_shape_disabled(RID p_area, int p_shape_idx, bool p_disabled) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	ERR_FAIL_INDEX(p_shape_idx, area->get_shape_count());
	FLUSH_QUERY_CHECK(area);
	area->set_shape_disabled(p_shape_idx, p_disabled);
}

void KaizenPhysicsServer3D::area_attach_object_instance_id(RID p_area, ObjectID p_id) {
	if (space_owner.owns(p_area)) {
		KaizenSpace3D *space = space_owner.get_or_null(p_area);
		p_area = space->get_default_area()->get_self();
	}
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->set_instance_id(p_id);
}

ObjectID KaizenPhysicsServer3D::area_get_object_instance_id(RID p_area) const {
	if (space_owner.owns(p_area)) {
		KaizenSpace3D *space = space_owner.get_or_null(p_area);
		p_area = space->get_default_area()->get_self();
	}
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, ObjectID());
	return area->get_instance_id();
}

void KaizenPhysicsServer3D::area_set_param(RID p_area, PS3DE::AreaParameter p_param, const Variant &p_value) {
	if (space_owner.owns(p_area)) {
		KaizenSpace3D *space = space_owner.get_or_null(p_area);
		p_area = space->get_default_area()->get_self();
	}
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->set_param(p_param, p_value);
}

void KaizenPhysicsServer3D::area_set_transform(RID p_area, const Transform3D &p_transform) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	area->set_transform(p_transform);
}

Variant KaizenPhysicsServer3D::area_get_param(RID p_area, PS3DE::AreaParameter p_param) const {
	if (space_owner.owns(p_area)) {
		KaizenSpace3D *space = space_owner.get_or_null(p_area);
		p_area = space->get_default_area()->get_self();
	}
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, Variant());

	return area->get_param(p_param);
}

Transform3D KaizenPhysicsServer3D::area_get_transform(RID p_area) const {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, Transform3D());

	return area->get_transform();
}

void KaizenPhysicsServer3D::area_set_collision_layer(RID p_area, uint32_t p_layer) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	area->set_collision_layer(p_layer);
}

uint32_t KaizenPhysicsServer3D::area_get_collision_layer(RID p_area) const {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, 0);

	return area->get_collision_layer();
}

void KaizenPhysicsServer3D::area_set_collision_mask(RID p_area, uint32_t p_mask) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	area->set_collision_mask(p_mask);
}

uint32_t KaizenPhysicsServer3D::area_get_collision_mask(RID p_area) const {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL_V(area, 0);

	return area->get_collision_mask();
}

void KaizenPhysicsServer3D::area_set_monitorable(RID p_area, bool p_monitorable) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);
	FLUSH_QUERY_CHECK(area);

	area->set_monitorable(p_monitorable);
}

void KaizenPhysicsServer3D::area_set_monitor_callback(RID p_area, const Callable &p_callback) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	area->set_monitor_callback(p_callback.is_valid() ? p_callback : Callable());
}

void KaizenPhysicsServer3D::area_set_ray_pickable(RID p_area, bool p_enable) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	area->set_ray_pickable(p_enable);
}

void KaizenPhysicsServer3D::area_set_area_monitor_callback(RID p_area, const Callable &p_callback) {
	KaizenArea3D *area = area_owner.get_or_null(p_area);
	ERR_FAIL_NULL(area);

	area->set_area_monitor_callback(p_callback.is_valid() ? p_callback : Callable());
}

/* BODY API */

RID KaizenPhysicsServer3D::body_create() {
	KaizenBody3D *body = memnew(KaizenBody3D);
	RID rid = body_owner.make_rid(body);
	body->set_self(rid);
	return rid;
}

void KaizenPhysicsServer3D::body_set_space(RID p_body, RID p_space) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	KaizenSpace3D *space = nullptr;
	if (p_space.is_valid()) {
		space = space_owner.get_or_null(p_space);
		ERR_FAIL_NULL(space);
	}

	if (body->get_space() == space) {
		return; //pointless
	}

	body->clear_constraint_map();
	body->set_space(space);
}

RID KaizenPhysicsServer3D::body_get_space(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, RID());

	KaizenSpace3D *space = body->get_space();
	if (!space) {
		return RID();
	}
	return space->get_self();
}

void KaizenPhysicsServer3D::body_set_mode(RID p_body, PS3DE::BodyMode p_mode) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_mode(p_mode);
}

PS3DE::BodyMode KaizenPhysicsServer3D::body_get_mode(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, PS3DE::BODY_MODE_STATIC);

	return body->get_mode();
}

void KaizenPhysicsServer3D::body_add_shape(RID p_body, RID p_shape, const Transform3D &p_transform, bool p_disabled) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(shape);

	body->add_shape(shape, p_transform, p_disabled);
}

void KaizenPhysicsServer3D::body_set_shape(RID p_body, int p_shape_idx, RID p_shape) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	KaizenShape3D *shape = shape_owner.get_or_null(p_shape);
	ERR_FAIL_NULL(shape);
	ERR_FAIL_COND(!shape->is_configured());

	body->set_shape(p_shape_idx, shape);
}
void KaizenPhysicsServer3D::body_set_shape_transform(RID p_body, int p_shape_idx, const Transform3D &p_transform) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_shape_transform(p_shape_idx, p_transform);
}

int KaizenPhysicsServer3D::body_get_shape_count(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, -1);

	return body->get_shape_count();
}

RID KaizenPhysicsServer3D::body_get_shape(RID p_body, int p_shape_idx) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, RID());

	KaizenShape3D *shape = body->get_shape(p_shape_idx);
	ERR_FAIL_NULL_V(shape, RID());

	return shape->get_self();
}

void KaizenPhysicsServer3D::body_set_shape_disabled(RID p_body, int p_shape_idx, bool p_disabled) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	ERR_FAIL_INDEX(p_shape_idx, body->get_shape_count());
	FLUSH_QUERY_CHECK(body);

	body->set_shape_disabled(p_shape_idx, p_disabled);
}

Transform3D KaizenPhysicsServer3D::body_get_shape_transform(RID p_body, int p_shape_idx) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Transform3D());

	return body->get_shape_transform(p_shape_idx);
}

void KaizenPhysicsServer3D::body_remove_shape(RID p_body, int p_shape_idx) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->remove_shape(p_shape_idx);
}

void KaizenPhysicsServer3D::body_clear_shapes(RID p_body) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	while (body->get_shape_count()) {
		body->remove_shape(0);
	}
}

void KaizenPhysicsServer3D::body_set_enable_continuous_collision_detection(RID p_body, bool p_enable) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_continuous_collision_detection(p_enable);
}

bool KaizenPhysicsServer3D::body_is_continuous_collision_detection_enabled(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);

	return body->is_continuous_collision_detection_enabled();
}

void KaizenPhysicsServer3D::body_set_collision_layer(RID p_body, uint32_t p_layer) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_collision_layer(p_layer);
}

uint32_t KaizenPhysicsServer3D::body_get_collision_layer(RID p_body) const {
	const KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);

	return body->get_collision_layer();
}

void KaizenPhysicsServer3D::body_set_collision_mask(RID p_body, uint32_t p_mask) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_collision_mask(p_mask);
}

uint32_t KaizenPhysicsServer3D::body_get_collision_mask(RID p_body) const {
	const KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);

	return body->get_collision_mask();
}

void KaizenPhysicsServer3D::body_set_collision_priority(RID p_body, real_t p_priority) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_collision_priority(p_priority);
}

real_t KaizenPhysicsServer3D::body_get_collision_priority(RID p_body) const {
	const KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);

	return body->get_collision_priority();
}

void KaizenPhysicsServer3D::body_attach_object_instance_id(RID p_body, ObjectID p_id) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	if (body) {
		body->set_instance_id(p_id);
		return;
	}

	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	if (soft_body) {
		soft_body->set_instance_id(p_id);
		return;
	}

	ERR_FAIL_MSG("Invalid ID.");
}

ObjectID KaizenPhysicsServer3D::body_get_object_instance_id(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, ObjectID());

	return body->get_instance_id();
}

void KaizenPhysicsServer3D::body_set_user_flags(RID p_body, uint32_t p_flags) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
}

uint32_t KaizenPhysicsServer3D::body_get_user_flags(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);

	return 0;
}

void KaizenPhysicsServer3D::body_set_param(RID p_body, PS3DE::BodyParameter p_param, const Variant &p_value) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_param(p_param, p_value);
}

Variant KaizenPhysicsServer3D::body_get_param(RID p_body, PS3DE::BodyParameter p_param) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);

	return body->get_param(p_param);
}

void KaizenPhysicsServer3D::body_reset_mass_properties(RID p_body) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	return body->reset_mass_properties();
}

void KaizenPhysicsServer3D::body_set_state(RID p_body, PS3DE::BodyState p_state, const Variant &p_variant) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_state(p_state, p_variant);
}

Variant KaizenPhysicsServer3D::body_get_state(RID p_body, PS3DE::BodyState p_state) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Variant());

	return body->get_state(p_state);
}

void KaizenPhysicsServer3D::body_apply_central_impulse(RID p_body, const Vector3 &p_impulse) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	_update_shapes();

	body->apply_central_impulse(p_impulse);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_apply_impulse(RID p_body, const Vector3 &p_impulse, const Vector3 &p_position) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	_update_shapes();

	body->apply_impulse(p_impulse, p_position);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_apply_torque_impulse(RID p_body, const Vector3 &p_impulse) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	_update_shapes();

	body->apply_torque_impulse(p_impulse);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_apply_central_force(RID p_body, const Vector3 &p_force) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->apply_central_force(p_force);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_apply_force(RID p_body, const Vector3 &p_force, const Vector3 &p_position) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->apply_force(p_force, p_position);
	body->wakeup();
}

void KaizenPhysicsServer3D::soft_body_apply_point_impulse(RID p_body, int p_point_index, const Vector3 &p_impulse) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->apply_node_impulse(p_point_index, p_impulse);
}

void KaizenPhysicsServer3D::soft_body_apply_point_force(RID p_body, int p_point_index, const Vector3 &p_force) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->apply_node_force(p_point_index, p_force);
}

void KaizenPhysicsServer3D::soft_body_apply_central_impulse(RID p_body, const Vector3 &p_impulse) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->apply_central_impulse(p_impulse);
}

void KaizenPhysicsServer3D::soft_body_apply_central_force(RID p_body, const Vector3 &p_force) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->apply_central_force(p_force);
}

void KaizenPhysicsServer3D::body_apply_torque(RID p_body, const Vector3 &p_torque) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->apply_torque(p_torque);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_add_constant_central_force(RID p_body, const Vector3 &p_force) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->add_constant_central_force(p_force);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_add_constant_force(RID p_body, const Vector3 &p_force, const Vector3 &p_position) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->add_constant_force(p_force, p_position);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_add_constant_torque(RID p_body, const Vector3 &p_torque) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->add_constant_torque(p_torque);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_set_constant_force(RID p_body, const Vector3 &p_force) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_constant_force(p_force);
	if (!p_force.is_zero_approx()) {
		body->wakeup();
	}
}

Vector3 KaizenPhysicsServer3D::body_get_constant_force(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Vector3());
	return body->get_constant_force();
}

void KaizenPhysicsServer3D::body_set_constant_torque(RID p_body, const Vector3 &p_torque) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_constant_torque(p_torque);
	if (!p_torque.is_zero_approx()) {
		body->wakeup();
	}
}

Vector3 KaizenPhysicsServer3D::body_get_constant_torque(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, Vector3());

	return body->get_constant_torque();
}

void KaizenPhysicsServer3D::body_set_axis_velocity(RID p_body, const Vector3 &p_axis_velocity) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	_update_shapes();

	Vector3 v = body->get_linear_velocity();
	Vector3 axis = p_axis_velocity.normalized();
	v -= axis * axis.dot(v);
	v += p_axis_velocity;
	body->set_linear_velocity(v);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_set_axis_lock(RID p_body, PS3DE::BodyAxis p_axis, bool p_lock) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_axis_lock(p_axis, p_lock);
	body->wakeup();
}

bool KaizenPhysicsServer3D::body_is_axis_locked(RID p_body, PS3DE::BodyAxis p_axis) const {
	const KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);
	return body->is_axis_locked(p_axis);
}

void KaizenPhysicsServer3D::body_add_collision_exception(RID p_body, RID p_body_b) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->add_exception(p_body_b);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_remove_collision_exception(RID p_body, RID p_body_b) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->remove_exception(p_body_b);
	body->wakeup();
}

void KaizenPhysicsServer3D::body_get_collision_exceptions(RID p_body, List<RID> *p_exceptions) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	for (int i = 0; i < body->get_exceptions().size(); i++) {
		p_exceptions->push_back(body->get_exceptions()[i]);
	}
}

void KaizenPhysicsServer3D::body_set_contacts_reported_depth_threshold(RID p_body, real_t p_threshold) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
}

real_t KaizenPhysicsServer3D::body_get_contacts_reported_depth_threshold(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, 0);
	return 0;
}

void KaizenPhysicsServer3D::body_set_omit_force_integration(RID p_body, bool p_omit) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);

	body->set_omit_force_integration(p_omit);
}

bool KaizenPhysicsServer3D::body_is_omitting_force_integration(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);
	return body->get_omit_force_integration();
}

void KaizenPhysicsServer3D::body_set_max_contacts_reported(RID p_body, int p_contacts) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->set_max_contacts_reported(p_contacts);
}

int KaizenPhysicsServer3D::body_get_max_contacts_reported(RID p_body) const {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, -1);
	return body->get_max_contacts_reported();
}

void KaizenPhysicsServer3D::body_set_state_sync_callback(RID p_body, const Callable &p_callable) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->set_state_sync_callback(p_callable);
}

void KaizenPhysicsServer3D::body_set_force_integration_callback(RID p_body, const Callable &p_callable, const Variant &p_udata) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->set_force_integration_callback(p_callable, p_udata);
}

void KaizenPhysicsServer3D::body_set_ray_pickable(RID p_body, bool p_enable) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(body);
	body->set_ray_pickable(p_enable);
}

bool KaizenPhysicsServer3D::body_test_motion(RID p_body, const PS3DT::MotionParameters &p_parameters, PS3DT::MotionResult *r_result) {
	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, false);
	ERR_FAIL_NULL_V(body->get_space(), false);
	ERR_FAIL_COND_V(body->get_space()->is_locked(), false);

	_update_shapes();

	return body->get_space()->test_body_motion(body, p_parameters, r_result);
}

PhysicsDirectBodyState3D *KaizenPhysicsServer3D::body_get_direct_state(RID p_body) {
	ERR_FAIL_COND_V_MSG((using_threads && !doing_sync), nullptr, "Body state is inaccessible right now, wait for iteration or physics process notification.");

	if (!body_owner.owns(p_body)) {
		return nullptr;
	}

	KaizenBody3D *body = body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(body, nullptr);

	if (!body->get_space()) {
		return nullptr;
	}

	ERR_FAIL_COND_V_MSG(body->get_space()->is_locked(), nullptr, "Body state is inaccessible right now, wait for iteration or physics process notification.");

	return body->get_direct_state();
}

/* SOFT BODY */

RID KaizenPhysicsServer3D::soft_body_create() {
	KaizenSoftBody3D *soft_body = memnew(KaizenSoftBody3D);
	RID rid = soft_body_owner.make_rid(soft_body);
	soft_body->set_self(rid);
	return rid;
}

void KaizenPhysicsServer3D::soft_body_update_rendering_server(RID p_body, RequiredParam<PhysicsServer3DRenderingServerHandler> p_rendering_server_handler) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);
	EXTRACT_PARAM_OR_FAIL(rendering_server_handler, p_rendering_server_handler);

	soft_body->update_rendering_server(rendering_server_handler);
}

void KaizenPhysicsServer3D::soft_body_set_space(RID p_body, RID p_space) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	KaizenSpace3D *space = nullptr;
	if (p_space.is_valid()) {
		space = space_owner.get_or_null(p_space);
		ERR_FAIL_NULL(space);
	}

	if (soft_body->get_space() == space) {
		return;
	}

	soft_body->set_space(space);
}

RID KaizenPhysicsServer3D::soft_body_get_space(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, RID());

	KaizenSpace3D *space = soft_body->get_space();
	if (!space) {
		return RID();
	}
	return space->get_self();
}

void KaizenPhysicsServer3D::soft_body_set_collision_layer(RID p_body, uint32_t p_layer) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_collision_layer(p_layer);
}

uint32_t KaizenPhysicsServer3D::soft_body_get_collision_layer(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0);

	return soft_body->get_collision_layer();
}

void KaizenPhysicsServer3D::soft_body_set_collision_mask(RID p_body, uint32_t p_mask) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_collision_mask(p_mask);
}

uint32_t KaizenPhysicsServer3D::soft_body_get_collision_mask(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0);

	return soft_body->get_collision_mask();
}

void KaizenPhysicsServer3D::soft_body_add_collision_exception(RID p_body, RID p_body_b) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->add_exception(p_body_b);
}

void KaizenPhysicsServer3D::soft_body_remove_collision_exception(RID p_body, RID p_body_b) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->remove_exception(p_body_b);
}

void KaizenPhysicsServer3D::soft_body_get_collision_exceptions(RID p_body, List<RID> *p_exceptions) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	for (int i = 0; i < soft_body->get_exceptions().size(); i++) {
		p_exceptions->push_back(soft_body->get_exceptions()[i]);
	}
}

void KaizenPhysicsServer3D::soft_body_set_state(RID p_body, PS3DE::BodyState p_state, const Variant &p_variant) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_state(p_state, p_variant);
}

Variant KaizenPhysicsServer3D::soft_body_get_state(RID p_body, PS3DE::BodyState p_state) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, Variant());

	return soft_body->get_state(p_state);
}

void KaizenPhysicsServer3D::soft_body_set_transform(RID p_body, const Transform3D &p_transform) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_state(PS3DE::BODY_STATE_TRANSFORM, p_transform);
}

void KaizenPhysicsServer3D::soft_body_set_ray_pickable(RID p_body, bool p_enable) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_ray_pickable(p_enable);
}

void KaizenPhysicsServer3D::soft_body_set_simulation_precision(RID p_body, int p_simulation_precision) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_iteration_count(p_simulation_precision);
}

int KaizenPhysicsServer3D::soft_body_get_simulation_precision(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0.f);

	return soft_body->get_iteration_count();
}

void KaizenPhysicsServer3D::soft_body_set_total_mass(RID p_body, real_t p_total_mass) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_total_mass(p_total_mass);
}

real_t KaizenPhysicsServer3D::soft_body_get_total_mass(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0.f);

	return soft_body->get_total_mass();
}

void KaizenPhysicsServer3D::soft_body_set_linear_stiffness(RID p_body, real_t p_stiffness) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_linear_stiffness(p_stiffness);
}

real_t KaizenPhysicsServer3D::soft_body_get_linear_stiffness(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0.f);

	return soft_body->get_linear_stiffness();
}

void KaizenPhysicsServer3D::soft_body_set_shrinking_factor(RID p_body, real_t p_shrinking_factor) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_shrinking_factor(p_shrinking_factor);
}

real_t KaizenPhysicsServer3D::soft_body_get_shrinking_factor(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0.f);

	return soft_body->get_shrinking_factor();
}

void KaizenPhysicsServer3D::soft_body_set_pressure_coefficient(RID p_body, real_t p_pressure_coefficient) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_pressure_coefficient(p_pressure_coefficient);
}

real_t KaizenPhysicsServer3D::soft_body_get_pressure_coefficient(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0.f);

	return soft_body->get_pressure_coefficient();
}

void KaizenPhysicsServer3D::soft_body_set_damping_coefficient(RID p_body, real_t p_damping_coefficient) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_damping_coefficient(p_damping_coefficient);
}

real_t KaizenPhysicsServer3D::soft_body_get_damping_coefficient(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0.f);

	return soft_body->get_damping_coefficient();
}

void KaizenPhysicsServer3D::soft_body_set_drag_coefficient(RID p_body, real_t p_drag_coefficient) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_drag_coefficient(p_drag_coefficient);
}

real_t KaizenPhysicsServer3D::soft_body_get_drag_coefficient(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, 0.f);

	return soft_body->get_drag_coefficient();
}

void KaizenPhysicsServer3D::soft_body_set_mesh(RID p_body, RID p_mesh) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_mesh(p_mesh);
}

AABB KaizenPhysicsServer3D::soft_body_get_bounds(RID p_body) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, AABB());

	return soft_body->get_bounds();
}

void KaizenPhysicsServer3D::soft_body_move_point(RID p_body, int p_point_index, const Vector3 &p_global_position) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->set_vertex_position(p_point_index, p_global_position);
}

Vector3 KaizenPhysicsServer3D::soft_body_get_point_global_position(RID p_body, int p_point_index) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, Vector3());

	return soft_body->get_vertex_position(p_point_index);
}

void KaizenPhysicsServer3D::soft_body_remove_all_pinned_points(RID p_body) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	soft_body->unpin_all_vertices();
}

void KaizenPhysicsServer3D::soft_body_pin_point(RID p_body, int p_point_index, bool p_pin) {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL(soft_body);

	if (p_pin) {
		soft_body->pin_vertex(p_point_index);
	} else {
		soft_body->unpin_vertex(p_point_index);
	}
}

bool KaizenPhysicsServer3D::soft_body_is_point_pinned(RID p_body, int p_point_index) const {
	KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_body);
	ERR_FAIL_NULL_V(soft_body, false);

	return soft_body->is_vertex_pinned(p_point_index);
}

/* JOINT API */

RID KaizenPhysicsServer3D::joint_create() {
	KaizenJoint3D *joint = memnew(KaizenJoint3D);
	RID rid = joint_owner.make_rid(joint);
	joint->set_self(rid);
	return rid;
}

void KaizenPhysicsServer3D::joint_clear(RID p_joint) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	if (joint->get_type() != PS3DE::JOINT_TYPE_MAX) {
		KaizenJoint3D *empty_joint = memnew(KaizenJoint3D);
		empty_joint->copy_settings_from(joint);

		joint_owner.replace(p_joint, empty_joint);
		memdelete(joint);
	}
}

void KaizenPhysicsServer3D::joint_make_pin(RID p_joint, RID p_body_A, const Vector3 &p_local_A, RID p_body_B, const Vector3 &p_local_B) {
	KaizenBody3D *body_A = body_owner.get_or_null(p_body_A);
	ERR_FAIL_NULL(body_A);

	if (!p_body_B.is_valid()) {
		ERR_FAIL_NULL(body_A->get_space());
		p_body_B = body_A->get_space()->get_static_global_body();
	}

	KaizenBody3D *body_B = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(body_B);

	ERR_FAIL_COND(body_A == body_B);

	KaizenJoint3D *prev_joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(prev_joint);

	KaizenJoint3D *joint = memnew(KaizenPinJoint3D(body_A, p_local_A, body_B, p_local_B));

	joint->copy_settings_from(prev_joint);
	joint_owner.replace(p_joint, joint);
	memdelete(prev_joint);
}

void KaizenPhysicsServer3D::pin_joint_set_param(RID p_joint, PS3DE::PinJointParam p_param, real_t p_value) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_PIN);
	KaizenPinJoint3D *pin_joint = static_cast<KaizenPinJoint3D *>(joint);
	pin_joint->set_param(p_param, p_value);
}

real_t KaizenPhysicsServer3D::pin_joint_get_param(RID p_joint, PS3DE::PinJointParam p_param) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_PIN, 0);
	KaizenPinJoint3D *pin_joint = static_cast<KaizenPinJoint3D *>(joint);
	return pin_joint->get_param(p_param);
}

void KaizenPhysicsServer3D::pin_joint_set_local_a(RID p_joint, const Vector3 &p_A) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_PIN);
	KaizenPinJoint3D *pin_joint = static_cast<KaizenPinJoint3D *>(joint);
	pin_joint->set_pos_a(p_A);
}

Vector3 KaizenPhysicsServer3D::pin_joint_get_local_a(RID p_joint) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, Vector3());
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_PIN, Vector3());
	KaizenPinJoint3D *pin_joint = static_cast<KaizenPinJoint3D *>(joint);
	return pin_joint->get_position_a();
}

void KaizenPhysicsServer3D::pin_joint_set_local_b(RID p_joint, const Vector3 &p_B) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_PIN);
	KaizenPinJoint3D *pin_joint = static_cast<KaizenPinJoint3D *>(joint);
	pin_joint->set_pos_b(p_B);
}

Vector3 KaizenPhysicsServer3D::pin_joint_get_local_b(RID p_joint) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, Vector3());
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_PIN, Vector3());
	KaizenPinJoint3D *pin_joint = static_cast<KaizenPinJoint3D *>(joint);
	return pin_joint->get_position_b();
}

void KaizenPhysicsServer3D::joint_make_hinge(RID p_joint, RID p_body_A, const Transform3D &p_frame_A, RID p_body_B, const Transform3D &p_frame_B) {
	KaizenBody3D *body_A = body_owner.get_or_null(p_body_A);
	ERR_FAIL_NULL(body_A);

	if (!p_body_B.is_valid()) {
		ERR_FAIL_NULL(body_A->get_space());
		p_body_B = body_A->get_space()->get_static_global_body();
	}

	KaizenBody3D *body_B = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(body_B);

	ERR_FAIL_COND(body_A == body_B);

	KaizenJoint3D *prev_joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(prev_joint);

	KaizenJoint3D *joint = memnew(KaizenHingeJoint3D(body_A, body_B, p_frame_A, p_frame_B));

	joint->copy_settings_from(prev_joint);
	joint_owner.replace(p_joint, joint);
	memdelete(prev_joint);
}

void KaizenPhysicsServer3D::joint_make_hinge_simple(RID p_joint, RID p_body_A, const Vector3 &p_pivot_A, const Vector3 &p_axis_A, RID p_body_B, const Vector3 &p_pivot_B, const Vector3 &p_axis_B) {
	KaizenBody3D *body_A = body_owner.get_or_null(p_body_A);
	ERR_FAIL_NULL(body_A);

	if (!p_body_B.is_valid()) {
		ERR_FAIL_NULL(body_A->get_space());
		p_body_B = body_A->get_space()->get_static_global_body();
	}

	KaizenBody3D *body_B = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(body_B);

	ERR_FAIL_COND(body_A == body_B);

	KaizenJoint3D *prev_joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(prev_joint);

	KaizenJoint3D *joint = memnew(KaizenHingeJoint3D(body_A, body_B, p_pivot_A, p_pivot_B, p_axis_A, p_axis_B));

	joint->copy_settings_from(prev_joint);
	joint_owner.replace(p_joint, joint);
	memdelete(prev_joint);
}

void KaizenPhysicsServer3D::hinge_joint_set_param(RID p_joint, PS3DE::HingeJointParam p_param, real_t p_value) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_HINGE);
	KaizenHingeJoint3D *hinge_joint = static_cast<KaizenHingeJoint3D *>(joint);
	hinge_joint->set_param(p_param, p_value);
}

real_t KaizenPhysicsServer3D::hinge_joint_get_param(RID p_joint, PS3DE::HingeJointParam p_param) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_HINGE, 0);
	KaizenHingeJoint3D *hinge_joint = static_cast<KaizenHingeJoint3D *>(joint);
	return hinge_joint->get_param(p_param);
}

void KaizenPhysicsServer3D::hinge_joint_set_flag(RID p_joint, PS3DE::HingeJointFlag p_flag, bool p_enabled) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_HINGE);
	KaizenHingeJoint3D *hinge_joint = static_cast<KaizenHingeJoint3D *>(joint);
	hinge_joint->set_flag(p_flag, p_enabled);
}

bool KaizenPhysicsServer3D::hinge_joint_get_flag(RID p_joint, PS3DE::HingeJointFlag p_flag) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, false);
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_HINGE, false);
	KaizenHingeJoint3D *hinge_joint = static_cast<KaizenHingeJoint3D *>(joint);
	return hinge_joint->get_flag(p_flag);
}

void KaizenPhysicsServer3D::joint_set_solver_priority(RID p_joint, int p_priority) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	joint->set_priority(p_priority);
}

int KaizenPhysicsServer3D::joint_get_solver_priority(RID p_joint) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	return joint->get_priority();
}

void KaizenPhysicsServer3D::joint_disable_collisions_between_bodies(RID p_joint, bool p_disable) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);

	joint->disable_collisions_between_bodies(p_disable);

	if (2 == joint->get_body_count()) {
		KaizenBody3D *body_a = *joint->get_body_ptr();
		KaizenBody3D *body_b = *(joint->get_body_ptr() + 1);

		if (p_disable) {
			body_add_collision_exception(body_a->get_self(), body_b->get_self());
			body_add_collision_exception(body_b->get_self(), body_a->get_self());
		} else {
			body_remove_collision_exception(body_a->get_self(), body_b->get_self());
			body_remove_collision_exception(body_b->get_self(), body_a->get_self());
		}
	}
}

bool KaizenPhysicsServer3D::joint_is_disabled_collisions_between_bodies(RID p_joint) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, true);

	return joint->is_disabled_collisions_between_bodies();
}

PS3DE::JointType KaizenPhysicsServer3D::joint_get_type(RID p_joint) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, PS3DE::JOINT_TYPE_PIN);
	return joint->get_type();
}

void KaizenPhysicsServer3D::joint_make_slider(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) {
	KaizenBody3D *body_A = body_owner.get_or_null(p_body_A);
	ERR_FAIL_NULL(body_A);

	if (!p_body_B.is_valid()) {
		ERR_FAIL_NULL(body_A->get_space());
		p_body_B = body_A->get_space()->get_static_global_body();
	}

	KaizenBody3D *body_B = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(body_B);

	ERR_FAIL_COND(body_A == body_B);

	KaizenJoint3D *prev_joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(prev_joint);

	KaizenJoint3D *joint = memnew(KaizenSliderJoint3D(body_A, body_B, p_local_frame_A, p_local_frame_B));

	joint->copy_settings_from(prev_joint);
	joint_owner.replace(p_joint, joint);
	memdelete(prev_joint);
}

void KaizenPhysicsServer3D::slider_joint_set_param(RID p_joint, PS3DE::SliderJointParam p_param, real_t p_value) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_SLIDER);
	KaizenSliderJoint3D *slider_joint = static_cast<KaizenSliderJoint3D *>(joint);
	slider_joint->set_param(p_param, p_value);
}

real_t KaizenPhysicsServer3D::slider_joint_get_param(RID p_joint, PS3DE::SliderJointParam p_param) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_SLIDER, 0);
	KaizenSliderJoint3D *slider_joint = static_cast<KaizenSliderJoint3D *>(joint);
	return slider_joint->get_param(p_param);
}

void KaizenPhysicsServer3D::joint_make_cone_twist(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) {
	KaizenBody3D *body_A = body_owner.get_or_null(p_body_A);
	ERR_FAIL_NULL(body_A);

	if (!p_body_B.is_valid()) {
		ERR_FAIL_NULL(body_A->get_space());
		p_body_B = body_A->get_space()->get_static_global_body();
	}

	KaizenBody3D *body_B = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(body_B);

	ERR_FAIL_COND(body_A == body_B);

	KaizenJoint3D *prev_joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(prev_joint);

	KaizenJoint3D *joint = memnew(KaizenConeTwistJoint3D(body_A, body_B, p_local_frame_A, p_local_frame_B));

	joint->copy_settings_from(prev_joint);
	joint_owner.replace(p_joint, joint);
	memdelete(prev_joint);
}

void KaizenPhysicsServer3D::cone_twist_joint_set_param(RID p_joint, PS3DE::ConeTwistJointParam p_param, real_t p_value) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_CONE_TWIST);
	KaizenConeTwistJoint3D *cone_twist_joint = static_cast<KaizenConeTwistJoint3D *>(joint);
	cone_twist_joint->set_param(p_param, p_value);
}

real_t KaizenPhysicsServer3D::cone_twist_joint_get_param(RID p_joint, PS3DE::ConeTwistJointParam p_param) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_CONE_TWIST, 0);
	KaizenConeTwistJoint3D *cone_twist_joint = static_cast<KaizenConeTwistJoint3D *>(joint);
	return cone_twist_joint->get_param(p_param);
}

void KaizenPhysicsServer3D::joint_make_generic_6dof(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) {
	KaizenBody3D *body_A = body_owner.get_or_null(p_body_A);
	ERR_FAIL_NULL(body_A);

	if (!p_body_B.is_valid()) {
		ERR_FAIL_NULL(body_A->get_space());
		p_body_B = body_A->get_space()->get_static_global_body();
	}

	KaizenBody3D *body_B = body_owner.get_or_null(p_body_B);
	ERR_FAIL_NULL(body_B);

	ERR_FAIL_COND(body_A == body_B);

	KaizenJoint3D *prev_joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(prev_joint);

	KaizenJoint3D *joint = memnew(KaizenGeneric6DOFJoint3D(body_A, body_B, p_local_frame_A, p_local_frame_B, true));

	joint->copy_settings_from(prev_joint);
	joint_owner.replace(p_joint, joint);
	memdelete(prev_joint);
}

void KaizenPhysicsServer3D::generic_6dof_joint_set_param(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisParam p_param, real_t p_value) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_6DOF);
	KaizenGeneric6DOFJoint3D *generic_6dof_joint = static_cast<KaizenGeneric6DOFJoint3D *>(joint);
	generic_6dof_joint->set_param(p_axis, p_param, p_value);
}

real_t KaizenPhysicsServer3D::generic_6dof_joint_get_param(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisParam p_param) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, 0);
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_6DOF, 0);
	KaizenGeneric6DOFJoint3D *generic_6dof_joint = static_cast<KaizenGeneric6DOFJoint3D *>(joint);
	return generic_6dof_joint->get_param(p_axis, p_param);
}

void KaizenPhysicsServer3D::generic_6dof_joint_set_flag(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisFlag p_flag, bool p_enable) {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_6DOF);
	KaizenGeneric6DOFJoint3D *generic_6dof_joint = static_cast<KaizenGeneric6DOFJoint3D *>(joint);
	generic_6dof_joint->set_flag(p_axis, p_flag, p_enable);
}

bool KaizenPhysicsServer3D::generic_6dof_joint_get_flag(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisFlag p_flag) const {
	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, false);
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_6DOF, false);
	KaizenGeneric6DOFJoint3D *generic_6dof_joint = static_cast<KaizenGeneric6DOFJoint3D *>(joint);
	return generic_6dof_joint->get_flag(p_axis, p_flag);
}

void KaizenPhysicsServer3D::generic_6dof_joint_set_angular_target_rotation(RID p_joint, const Quaternion &p_target_rotation) {
	ERR_FAIL_COND_MSG(!p_target_rotation.is_finite() || p_target_rotation.length_squared() <= CMP_EPSILON, "Angular target rotation must be a finite, non-zero quaternion.");

	KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL(joint);
	ERR_FAIL_COND(joint->get_type() != PS3DE::JOINT_TYPE_6DOF);
	WARN_PRINT_ONCE("Quaternion angular target rotations for Generic6DOFJoint3D are only supported by Jolt Physics. This value will be ignored by KaizenPhysics3D.");
}

Quaternion KaizenPhysicsServer3D::generic_6dof_joint_get_angular_target_rotation(RID p_joint) const {
	const KaizenJoint3D *joint = joint_owner.get_or_null(p_joint);
	ERR_FAIL_NULL_V(joint, Quaternion());
	ERR_FAIL_COND_V(joint->get_type() != PS3DE::JOINT_TYPE_6DOF, Quaternion());

	// KaizenPhysics3D does not apply angular spring equilibrium points, so there is no body-space target to return.
	WARN_PRINT_ONCE("Quaternion angular target rotations for Generic6DOFJoint3D are only supported by Jolt Physics. KaizenPhysics3D returns the identity quaternion.");
	return Quaternion();
}

void KaizenPhysicsServer3D::free_rid(RID p_rid) {
	_update_shapes(); //just in case

	if (shape_owner.owns(p_rid)) {
		KaizenShape3D *shape = shape_owner.get_or_null(p_rid);

		while (shape->get_owners().size()) {
			KaizenShapeOwner3D *so = shape->get_owners().begin()->key;
			so->remove_shape(shape);
		}

		shape_owner.free(p_rid);
		memdelete(shape);
	} else if (body_owner.owns(p_rid)) {
		KaizenBody3D *body = body_owner.get_or_null(p_rid);

		body->set_space(nullptr);

		while (body->get_shape_count()) {
			body->remove_shape(0);
		}

		body_owner.free(p_rid);
		memdelete(body);
	} else if (soft_body_owner.owns(p_rid)) {
		KaizenSoftBody3D *soft_body = soft_body_owner.get_or_null(p_rid);

		soft_body->set_space(nullptr);

		soft_body_owner.free(p_rid);
		memdelete(soft_body);
	} else if (area_owner.owns(p_rid)) {
		KaizenArea3D *area = area_owner.get_or_null(p_rid);

		area->set_space(nullptr);

		while (area->get_shape_count()) {
			area->remove_shape(0);
		}

		area_owner.free(p_rid);
		memdelete(area);
	} else if (space_owner.owns(p_rid)) {
		KaizenSpace3D *space = space_owner.get_or_null(p_rid);

		while (space->get_objects().size()) {
			KaizenCollisionObject3D *co = static_cast<KaizenCollisionObject3D *>(*space->get_objects().begin());
			co->set_space(nullptr);
		}

		active_spaces.erase(space);
		free_rid(space->get_default_area()->get_self());
		free_rid(space->get_static_global_body());

		space_owner.free(p_rid);
		memdelete(space);
	} else if (joint_owner.owns(p_rid)) {
		KaizenJoint3D *joint = joint_owner.get_or_null(p_rid);

		joint_owner.free(p_rid);
		memdelete(joint);

	} else {
		ERR_FAIL_MSG("Invalid ID.");
	}
}

void KaizenPhysicsServer3D::set_active(bool p_active) {
	active = p_active;
}

void KaizenPhysicsServer3D::init() {
	stepper = memnew(KaizenStep3D);
}

void KaizenPhysicsServer3D::step(real_t p_step) {
	if (!active) {
		return;
	}

	_update_shapes();

	island_count = 0;
	active_objects = 0;
	collision_pairs = 0;
	for (KaizenSpace3D *E : active_spaces) {
		stepper->step(E, p_step);
		island_count += E->get_island_count();
		active_objects += E->get_active_objects();
		collision_pairs += E->get_collision_pairs();
	}
}

void KaizenPhysicsServer3D::sync() {
	doing_sync = true;
}

void KaizenPhysicsServer3D::flush_queries() {
	if (!active) {
		return;
	}

	flushing_queries = true;

	uint64_t time_beg = OS::get_singleton()->get_ticks_usec();

	for (KaizenSpace3D *E : active_spaces) {
		KaizenSpace3D *space = E;
		space->call_queries();
	}

	flushing_queries = false;

	if (EngineDebugger::is_profiling("servers")) {
		uint64_t total_time[KaizenSpace3D::ELAPSED_TIME_MAX];
		static const char *time_name[KaizenSpace3D::ELAPSED_TIME_MAX] = {
			"integrate_forces",
			"generate_islands",
			"setup_constraints",
			"solve_constraints",
			"integrate_velocities"
		};

		for (int i = 0; i < KaizenSpace3D::ELAPSED_TIME_MAX; i++) {
			total_time[i] = 0;
		}

		for (const KaizenSpace3D *E : active_spaces) {
			for (int i = 0; i < KaizenSpace3D::ELAPSED_TIME_MAX; i++) {
				total_time[i] += E->get_elapsed_time(KaizenSpace3D::ElapsedTime(i));
			}
		}

		Array values;
		values.resize(KaizenSpace3D::ELAPSED_TIME_MAX * 2);
		for (int i = 0; i < KaizenSpace3D::ELAPSED_TIME_MAX; i++) {
			values[i * 2 + 0] = time_name[i];
			values[i * 2 + 1] = USEC_TO_SEC(total_time[i]);
		}
		values.push_back("flush_queries");
		values.push_back(USEC_TO_SEC(OS::get_singleton()->get_ticks_usec() - time_beg));

		values.push_front("physics_3d");
		EngineDebugger::profiler_add_frame_data("servers", values);
	}
}

void KaizenPhysicsServer3D::end_sync() {
	doing_sync = false;
}

void KaizenPhysicsServer3D::finish() {
	memdelete(stepper);
}

int KaizenPhysicsServer3D::get_process_info(PS3DE::ProcessInfo p_info) {
	switch (p_info) {
		case PS3DE::INFO_ACTIVE_OBJECTS: {
			return active_objects;
		} break;
		case PS3DE::INFO_COLLISION_PAIRS: {
			return collision_pairs;
		} break;
		case PS3DE::INFO_ISLAND_COUNT: {
			return island_count;
		} break;
	}

	return 0;
}

void KaizenPhysicsServer3D::_update_shapes() {
	while (pending_shape_update_list.first()) {
		pending_shape_update_list.first()->self()->_shape_changed();
		pending_shape_update_list.remove(pending_shape_update_list.first());
	}
}

void KaizenPhysicsServer3D::_shape_col_cbk(const Vector3 &p_point_A, int p_index_A, const Vector3 &p_point_B, int p_index_B, const Vector3 &normal, void *p_userdata) {
	CollCbkData *cbk = static_cast<CollCbkData *>(p_userdata);

	if (cbk->max == 0) {
		return;
	}

	if (cbk->amount == cbk->max) {
		//find least deep
		real_t min_depth = 1e20;
		int min_depth_idx = 0;
		for (int i = 0; i < cbk->amount; i++) {
			real_t d = cbk->ptr[i * 2 + 0].distance_squared_to(cbk->ptr[i * 2 + 1]);
			if (d < min_depth) {
				min_depth = d;
				min_depth_idx = i;
			}
		}

		real_t d = p_point_A.distance_squared_to(p_point_B);
		if (d < min_depth) {
			return;
		}
		cbk->ptr[min_depth_idx * 2 + 0] = p_point_A;
		cbk->ptr[min_depth_idx * 2 + 1] = p_point_B;

	} else {
		cbk->ptr[cbk->amount * 2 + 0] = p_point_A;
		cbk->ptr[cbk->amount * 2 + 1] = p_point_B;
		cbk->amount++;
	}
}

KaizenPhysicsServer3D *KaizenPhysicsServer3D::kaizen_singleton = nullptr;
KaizenPhysicsServer3D::KaizenPhysicsServer3D(bool p_using_threads) {
	kaizen_singleton = this;
	KaizenBroadPhase3D::create_func = KaizenBroadPhase3DBVH::_create;

	using_threads = p_using_threads;
}
