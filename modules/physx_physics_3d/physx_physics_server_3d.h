//************************************************************************//
//  physx_physics_server_3d.h                                             //
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

#pragma once

#include "core/templates/rid_owner.h"
#include "kaizen/physics/physx/physx_backend.h"
#include "servers/physics_3d/physics_server_3d_dummy.h"

#include <PxPhysicsAPI.h>

/// @brief "PhysX Physics" server: a real PhysicsServer3D backed by the PhysX 5 SDK.
///
/// Coverage (implemented against the real SDK):
///  - Shapes: world boundary, sphere, box, capsule, cylinder (convex hull),
///    convex polygon, concave polygon (triangle mesh), heightmap, separation ray
///    (thin box approximation).
///  - Spaces: per-space PxScene with gravity and fixed-step simulation.
///  - Bodies: static/kinematic/rigid/rigid-linear, mass properties, velocities,
///    forces/impulses, axis locking, CCD, collision layer/mask/exceptions.
///  - Areas: trigger bodies with monitor callbacks (per-step overlap diffing).
///  - Queries: raycast, point/shape overlap, shape collide, rest info, motion test.
///  - Joints: pin (spherical), hinge (revolute), slider (prismatic), cone-twist
///    and generic 6DOF (D6) with limits, motors and springs.
///
/// Not covered (inherits PhysicsServer3DDummy no-op behavior, warned on create):
///  - Soft bodies, custom shapes, heightmap per-sample editing.
class PhysXPhysicsServer3D : public PhysicsServer3DDummy {
	GDCLASS(PhysXPhysicsServer3D, PhysicsServer3DDummy);

	// Owner header shared by bodies and areas (stored in PxRigidActor::userData).
	struct PhysXObject {
		enum Kind { BODY,
			AREA } kind = BODY;
		RID rid;
		ObjectID instance_id;
	};

	struct PhysXShape {
		RID rid;
		PS3DE::ShapeType type = PS3DE::SHAPE_SPHERE;
		Variant data;
		real_t margin = 0.04;
		real_t custom_solver_bias = 0.0;
		physx::PxGeometryHolder geometry; // Built from data.
		physx::PxConvexMesh *convex_mesh = nullptr; // Cooked (convex/cylinder).
		physx::PxTriangleMesh *triangle_mesh = nullptr; // Cooked (concave).
		physx::PxHeightField *height_field = nullptr; // Cooked (heightmap).
		bool valid = false;

		~PhysXShape();
	};

	struct ShapeInstance {
		RID shape_rid;
		Transform3D local_transform;
		bool disabled = false;
		physx::PxShape *px_shape = nullptr;
	};

	struct PhysXSpace {
		RID rid;
		physx::PxScene *scene = nullptr;
		Vector3 gravity = Vector3(0, -9.81, 0);
		real_t gravity_magnitude = 9.81;
		Vector3 gravity_direction = Vector3(0, -1, 0);
		real_t default_linear_damp = 0.0;
		real_t default_angular_damp = 0.0;
		bool active = false;
		HashMap<int, real_t> params;
		int max_debug_contacts = 0;
		Vector<Vector3> debug_contacts;
	};

	struct PhysXBody : PhysXObject {
		physx::PxRigidActor *actor = nullptr;
		PS3DE::BodyMode mode = PS3DE::BODY_MODE_STATIC;
		RID space;
		Vector<ShapeInstance> shapes;
		uint32_t collision_layer = 1;
		uint32_t collision_mask = 1;
		real_t collision_priority = 1.0;
		uint32_t user_flags = 0;
		bool ccd = false;
		bool ray_pickable = true;
		bool omit_force_integration = false;
		ObjectID object_id;
		Vector3 constant_force;
		Vector3 constant_torque;
		HashSet<RID> exceptions;
		int max_contacts_reported = 0;
		real_t contacts_depth_threshold = 0;
		Callable state_sync_callback;
		Callable force_integration_callback;
		Variant force_integration_udata;
		// Cached params (PhysX stores only a subset natively).
		Variant params[PS3DE::BODY_PARAM_MAX];
		bool param_set[PS3DE::BODY_PARAM_MAX] = {};

		PhysXBody() { kind = BODY; }
	};

	struct PhysXArea : PhysXObject {
		physx::PxRigidStatic *actor = nullptr;
		RID space;
		Vector<ShapeInstance> shapes;
		uint32_t collision_layer = 1;
		uint32_t collision_mask = 1;
		bool monitorable = true;
		bool ray_pickable = true;
		real_t priority = 0;
		PS3DE::AreaSpaceOverrideMode gravity_override_mode = PS3DE::AREA_SPACE_OVERRIDE_DISABLED;
		real_t gravity = 0;
		Vector3 gravity_vector = Vector3(0, -1, 0);
		bool gravity_is_point = false;
		real_t gravity_point_unit_distance = 0;
		PS3DE::AreaSpaceOverrideMode linear_damp_override_mode = PS3DE::AREA_SPACE_OVERRIDE_DISABLED;
		real_t linear_damp = 0;
		PS3DE::AreaSpaceOverrideMode angular_damp_override_mode = PS3DE::AREA_SPACE_OVERRIDE_DISABLED;
		real_t angular_damp = 0;
		real_t wind_force_magnitude = 0;
		Vector3 wind_source;
		Vector3 wind_direction;
		real_t wind_attenuation_factor = 0;
		Callable monitor_callback;
		Callable area_monitor_callback;
		// Overlap tracking (per-step diffing).
		HashMap<RID, LocalVector<int>> overlapping_bodies; // body RID -> shape indices.
		HashMap<RID, LocalVector<int>> overlapping_areas;

		PhysXArea() { kind = AREA; }
	};

	struct PhysXJoint {
		RID rid;
		PS3DE::JointType type = PS3DE::JOINT_TYPE_PIN;
		physx::PxJoint *px_joint = nullptr;
		RID body_a;
		RID body_b;
		int solver_priority = 1;
		bool disable_collisions = true;
		// Cached params.
		HashMap<int, real_t> pin_params;
		HashMap<int, real_t> hinge_params;
		HashMap<int, real_t> slider_params;
		HashMap<int, real_t> cone_twist_params;
		HashMap<int, real_t> g6dof_params; // key = axis * 100 + param.
		HashMap<int, bool> g6dof_flags;
		Quaternion g6dof_target_rotation;
	};

	// Direct states.
	class PhysXDirectBodyState;
	class PhysXDirectSpaceState;

	mutable RID_Owner<PhysXShape, true> shape_owner;
	mutable RID_Owner<PhysXSpace, true> space_owner;
	mutable RID_Owner<PhysXBody, true> body_owner;
	mutable RID_Owner<PhysXArea, true> area_owner;
	mutable RID_Owner<PhysXJoint, true> joint_owner;

	Ref<PhysXBackend> backend;
	RID default_space;
	HashMap<RID, PhysXBody *> body_map; // RID -> body (fast reverse lookup).
	HashMap<RID, PhysXArea *> area_map;
	HashMap<RID, PhysXShape *> shape_map;
	HashMap<RID, PhysXSpace *> space_map;
	HashMap<RID, PhysXJoint *> joint_map;
	HashMap<RID, PhysXDirectBodyState *> direct_body_states;
	HashMap<RID, PhysXDirectSpaceState *> direct_space_states;

	// User callbacks run while stepping; free_rid() defers the actual release
	// to the end of the step so map iteration and in-flight objects stay valid.
	bool active = true;
	bool flushing_queries = false;
	int active_objects = 0;
	int collision_pairs = 0;
	bool in_step = false;
	Vector<RID> pending_frees;

	// Shape cooking helpers.
	bool _build_shape_geometry(PhysXShape *p_shape);
	RID _alloc_shape(PS3DE::ShapeType p_type);
	void _body_add_px_shape(PhysXBody *p_body, int p_idx);
	void _body_remove_px_shape(PhysXBody *p_body, int p_idx);
	void _body_rebuild_shapes(PhysXBody *p_body);
	void _area_add_px_shape(PhysXArea *p_area, int p_idx);
	void _area_remove_px_shape(PhysXArea *p_area, int p_idx);
	void _area_rebuild_shapes(PhysXArea *p_area);
	void _resync_px_shape_indices(PhysXBody *p_body);
	void _resync_px_shape_indices(PhysXArea *p_area);
	void _apply_body_mode(PhysXBody *p_body);
	void _apply_simulation_step_forces();
	void _apply_body_mass(PhysXBody *p_body);
	void _apply_body_damping(PhysXBody *p_body);
	Vector<PhysXArea *> _get_overlapping_areas(const PhysXBody *p_body) const;
	Vector3 _get_effective_gravity(const PhysXBody *p_body, const Vector3 &p_position) const;
	real_t _get_effective_damping(const PhysXBody *p_body, bool p_angular) const;
	void _apply_filter_data(PhysXBody *p_body);
	void _destroy_joint(PhysXJoint *p_joint);
	void _check_area_overlaps();

	physx::PxFilterData _make_filter_data(uint32_t p_layer, uint32_t p_mask, const PhysXObject *p_object) const;

public:
	PhysXPhysicsServer3D();
	~PhysXPhysicsServer3D() override;

	// ---- Shapes ----
	RID world_boundary_shape_create() override;
	RID separation_ray_shape_create() override;
	RID sphere_shape_create() override;
	RID box_shape_create() override;
	RID capsule_shape_create() override;
	RID cylinder_shape_create() override;
	RID convex_polygon_shape_create() override;
	RID concave_polygon_shape_create() override;
	RID heightmap_shape_create() override;

	void shape_set_data(RID p_shape, const Variant &p_data) override;
	void shape_set_custom_solver_bias(RID p_shape, real_t p_bias) override;
	PS3DE::ShapeType shape_get_type(RID p_shape) const override;
	Variant shape_get_data(RID p_shape) const override;
	void shape_set_margin(RID p_shape, real_t p_margin) override;
	real_t shape_get_margin(RID p_shape) const override;
	real_t shape_get_custom_solver_bias(RID p_shape) const override;

	// ---- Spaces ----
	RID space_create() override;
	void space_set_active(RID p_space, bool p_active) override;
	bool space_is_active(RID p_space) const override;
	void space_set_param(RID p_space, PS3DE::SpaceParameter p_param, real_t p_value) override;
	real_t space_get_param(RID p_space, PS3DE::SpaceParameter p_param) const override;
	PhysicsDirectSpaceState3D *space_get_direct_state(RID p_space) override;
	void space_set_debug_contacts(RID p_space, int p_max_contacts) override;
	Vector<Vector3> space_get_contacts(RID p_space) const override;
	int space_get_contact_count(RID p_space) const override;

	// ---- Areas ----
	RID area_create() override;
	void area_set_space(RID p_area, RID p_space) override;
	RID area_get_space(RID p_area) const override;
	void area_add_shape(RID p_area, RID p_shape, const Transform3D &p_transform = Transform3D(), bool p_disabled = false) override;
	void area_set_shape(RID p_area, int p_shape_idx, RID p_shape) override;
	void area_set_shape_transform(RID p_area, int p_shape_idx, const Transform3D &p_transform) override;
	int area_get_shape_count(RID p_area) const override;
	RID area_get_shape(RID p_area, int p_shape_idx) const override;
	Transform3D area_get_shape_transform(RID p_area, int p_shape_idx) const override;
	void area_remove_shape(RID p_area, int p_shape_idx) override;
	void area_clear_shapes(RID p_area) override;
	void area_set_shape_disabled(RID p_area, int p_shape_idx, bool p_disabled) override;
	void area_attach_object_instance_id(RID p_area, ObjectID p_id) override;
	ObjectID area_get_object_instance_id(RID p_area) const override;
	void area_set_param(RID p_area, PS3DE::AreaParameter p_param, const Variant &p_value) override;
	void area_set_transform(RID p_area, const Transform3D &p_transform) override;
	Variant area_get_param(RID p_parea, PS3DE::AreaParameter p_param) const override;
	Transform3D area_get_transform(RID p_area) const override;
	void area_set_collision_layer(RID p_area, uint32_t p_layer) override;
	uint32_t area_get_collision_layer(RID p_area) const override;
	void area_set_collision_mask(RID p_area, uint32_t p_mask) override;
	uint32_t area_get_collision_mask(RID p_area) const override;
	void area_set_monitorable(RID p_area, bool p_monitorable) override;
	void area_set_monitor_callback(RID p_area, const Callable &p_callback) override;
	void area_set_area_monitor_callback(RID p_area, const Callable &p_callback) override;
	void area_set_ray_pickable(RID p_area, bool p_enable) override;

	// ---- Bodies ----
	RID body_create() override;
	void body_set_space(RID p_body, RID p_space) override;
	RID body_get_space(RID p_body) const override;
	void body_set_mode(RID p_body, PS3DE::BodyMode p_mode) override;
	PS3DE::BodyMode body_get_mode(RID p_body) const override;
	void body_add_shape(RID p_body, RID p_shape, const Transform3D &p_transform = Transform3D(), bool p_disabled = false) override;
	void body_set_shape(RID p_body, int p_shape_idx, RID p_shape) override;
	void body_set_shape_transform(RID p_body, int p_shape_idx, const Transform3D &p_transform) override;
	int body_get_shape_count(RID p_body) const override;
	RID body_get_shape(RID p_body, int p_shape_idx) const override;
	Transform3D body_get_shape_transform(RID p_body, int p_shape_idx) const override;
	void body_remove_shape(RID p_body, int p_shape_idx) override;
	void body_clear_shapes(RID p_body) override;
	void body_set_shape_disabled(RID p_body, int p_shape_idx, bool p_disabled) override;
	void body_attach_object_instance_id(RID p_body, ObjectID p_id) override;
	ObjectID body_get_object_instance_id(RID p_body) const override;
	void body_set_enable_continuous_collision_detection(RID p_body, bool p_enable) override;
	bool body_is_continuous_collision_detection_enabled(RID p_body) const override;
	void body_set_collision_layer(RID p_body, uint32_t p_layer) override;
	uint32_t body_get_collision_layer(RID p_body) const override;
	void body_set_collision_mask(RID p_body, uint32_t p_mask) override;
	uint32_t body_get_collision_mask(RID p_body) const override;
	void body_set_collision_priority(RID p_body, real_t p_priority) override;
	real_t body_get_collision_priority(RID p_body) const override;
	void body_set_user_flags(RID p_body, uint32_t p_flags) override;
	uint32_t body_get_user_flags(RID p_body) const override;
	void body_set_param(RID p_body, PS3DE::BodyParameter p_param, const Variant &p_value) override;
	Variant body_get_param(RID p_body, PS3DE::BodyParameter p_param) const override;
	void body_reset_mass_properties(RID p_body) override;
	void body_set_state(RID p_body, PS3DE::BodyState p_state, const Variant &p_variant) override;
	Variant body_get_state(RID p_body, PS3DE::BodyState p_state) const override;
	void body_apply_central_impulse(RID p_body, const Vector3 &p_impulse) override;
	void body_apply_impulse(RID p_body, const Vector3 &p_impulse, const Vector3 &p_position = Vector3()) override;
	void body_apply_torque_impulse(RID p_body, const Vector3 &p_impulse) override;
	void body_apply_central_force(RID p_body, const Vector3 &p_force) override;
	void body_apply_force(RID p_body, const Vector3 &p_force, const Vector3 &p_position = Vector3()) override;
	void body_apply_torque(RID p_body, const Vector3 &p_torque) override;
	void body_add_constant_central_force(RID p_body, const Vector3 &p_force) override;
	void body_add_constant_force(RID p_body, const Vector3 &p_force, const Vector3 &p_position = Vector3()) override;
	void body_add_constant_torque(RID p_body, const Vector3 &p_torque) override;
	void body_set_constant_force(RID p_body, const Vector3 &p_force) override;
	Vector3 body_get_constant_force(RID p_body) const override;
	void body_set_constant_torque(RID p_body, const Vector3 &p_torque) override;
	Vector3 body_get_constant_torque(RID p_body) const override;
	void body_set_axis_velocity(RID p_body, const Vector3 &p_axis_velocity) override;
	void body_set_axis_lock(RID p_body, PS3DE::BodyAxis p_axis, bool p_lock) override;
	bool body_is_axis_locked(RID p_body, PS3DE::BodyAxis p_axis) const override;
	void body_add_collision_exception(RID p_body, RID p_body_b) override;
	void body_remove_collision_exception(RID p_body, RID p_body_b) override;
	void body_get_collision_exceptions(RID p_body, List<RID> *p_exceptions) override;
	void body_set_max_contacts_reported(RID p_body, int p_contacts) override;
	int body_get_max_contacts_reported(RID p_body) const override;
	void body_set_contacts_reported_depth_threshold(RID p_body, real_t p_threshold) override;
	real_t body_get_contacts_reported_depth_threshold(RID p_body) const override;
	void body_set_omit_force_integration(RID p_body, bool p_omit) override;
	bool body_is_omitting_force_integration(RID p_body) const override;
	void body_set_state_sync_callback(RID p_body, const Callable &p_callable) override;
	void body_set_force_integration_callback(RID p_body, const Callable &p_callable, const Variant &p_udata = Variant()) override;
	void body_set_ray_pickable(RID p_body, bool p_enable) override;
	PhysicsDirectBodyState3D *body_get_direct_state(RID p_body) override;
	bool body_test_motion(RID p_body, const PS3DT::MotionParameters &p_parameters, PS3DT::MotionResult *r_result = nullptr) override;

	// ---- Soft bodies (not supported by PhysX; warned) ----
	RID soft_body_create() override;

	// ---- Joints ----
	RID joint_create() override;
	void joint_clear(RID p_joint) override;
	PS3DE::JointType joint_get_type(RID p_joint) const override;
	void joint_set_solver_priority(RID p_joint, int p_priority) override;
	int joint_get_solver_priority(RID p_joint) const override;
	void joint_disable_collisions_between_bodies(RID p_joint, bool p_disable) override;
	bool joint_is_disabled_collisions_between_bodies(RID p_joint) const override;
	void joint_make_pin(RID p_joint, RID p_body_A, const Vector3 &p_local_A, RID p_body_B, const Vector3 &p_local_B) override;
	void pin_joint_set_param(RID p_joint, PS3DE::PinJointParam p_param, real_t p_value) override;
	real_t pin_joint_get_param(RID p_joint, PS3DE::PinJointParam p_param) const override;
	void pin_joint_set_local_a(RID p_joint, const Vector3 &p_A) override;
	Vector3 pin_joint_get_local_a(RID p_joint) const override;
	void pin_joint_set_local_b(RID p_joint, const Vector3 &p_B) override;
	Vector3 pin_joint_get_local_b(RID p_joint) const override;
	void joint_make_hinge(RID p_joint, RID p_body_A, const Transform3D &p_hinge_A, RID p_body_B, const Transform3D &p_hinge_B) override;
	void joint_make_hinge_simple(RID p_joint, RID p_body_A, const Vector3 &p_pivot_A, const Vector3 &p_axis_A, RID p_body_B, const Vector3 &p_pivot_B, const Vector3 &p_axis_B) override;
	void hinge_joint_set_param(RID p_joint, PS3DE::HingeJointParam p_param, real_t p_value) override;
	real_t hinge_joint_get_param(RID p_joint, PS3DE::HingeJointParam p_param) const override;
	void hinge_joint_set_flag(RID p_joint, PS3DE::HingeJointFlag p_flag, bool p_enabled) override;
	bool hinge_joint_get_flag(RID p_joint, PS3DE::HingeJointFlag p_flag) const override;
	void joint_make_slider(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) override;
	void slider_joint_set_param(RID p_joint, PS3DE::SliderJointParam p_param, real_t p_value) override;
	real_t slider_joint_get_param(RID p_joint, PS3DE::SliderJointParam p_param) const override;
	void joint_make_cone_twist(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) override;
	void cone_twist_joint_set_param(RID p_joint, PS3DE::ConeTwistJointParam p_param, real_t p_value) override;
	real_t cone_twist_joint_get_param(RID p_joint, PS3DE::ConeTwistJointParam p_param) const override;
	void joint_make_generic_6dof(RID p_joint, RID p_body_A, const Transform3D &p_local_frame_A, RID p_body_B, const Transform3D &p_local_frame_B) override;
	void generic_6dof_joint_set_param(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisParam p_param, real_t p_value) override;
	real_t generic_6dof_joint_get_param(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisParam p_param) const override;
	void generic_6dof_joint_set_flag(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisFlag p_flag, bool p_enable) override;
	bool generic_6dof_joint_get_flag(RID p_joint, Vector3::Axis p_axis, PS3DE::G6DOFJointAxisFlag p_flag) const override;
	void generic_6dof_joint_set_angular_target_rotation(RID p_joint, const Quaternion &p_target_rotation) override;
	Quaternion generic_6dof_joint_get_angular_target_rotation(RID p_joint) const override;

	// ---- Lifecycle ----
	void free_rid(RID p_rid) override;
	void set_active(bool p_active) override;
	bool is_flushing_queries() const override { return flushing_queries; }
	int get_process_info(PS3DE::ProcessInfo p_info) override;
	void init() override;
	void step(real_t p_step) override;
	void sync() override;
	void flush_queries() override;
	void end_sync() override;
	void finish() override;

	PhysXBackend *get_backend() const { return backend.ptr(); }
};
