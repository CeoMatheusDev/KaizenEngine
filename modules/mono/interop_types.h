/**************************************************************************/
/*  interop_types.h                                                       */
/**************************************************************************/
/*                         This file is part of:                          */
/*                             KAIZEN ENGINE                               */
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

#pragma once

#include "core/math/math_defs.h"

#ifdef __cplusplus
extern "C" {
#endif

// This is taken from the old GDNative, which was removed.

#define KAIZEN_VARIANT_SIZE (sizeof(real_t) * 4 + sizeof(int64_t))

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_VARIANT_SIZE];
} kaizen_variant;

#define KAIZEN_ARRAY_SIZE sizeof(void *)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_ARRAY_SIZE];
} kaizen_array;

#define KAIZEN_DICTIONARY_SIZE sizeof(void *)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_DICTIONARY_SIZE];
} kaizen_dictionary;

#define KAIZEN_STRING_SIZE sizeof(void *)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_STRING_SIZE];
} kaizen_string;

#define KAIZEN_STRING_NAME_SIZE sizeof(void *)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_STRING_NAME_SIZE];
} kaizen_string_name;

#define KAIZEN_PACKED_ARRAY_SIZE (2 * sizeof(void *))

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_PACKED_ARRAY_SIZE];
} kaizen_packed_array;

#define KAIZEN_VECTOR2_SIZE (sizeof(real_t) * 2)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_VECTOR2_SIZE];
} kaizen_vector2;

#define KAIZEN_VECTOR2I_SIZE (sizeof(int32_t) * 2)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_VECTOR2I_SIZE];
} kaizen_vector2i;

#define KAIZEN_RECT2_SIZE (sizeof(real_t) * 4)

typedef struct kaizen_rect2 {
	uint8_t _dont_touch_that[KAIZEN_RECT2_SIZE];
} kaizen_rect2;

#define KAIZEN_RECT2I_SIZE (sizeof(int32_t) * 4)

typedef struct kaizen_rect2i {
	uint8_t _dont_touch_that[KAIZEN_RECT2I_SIZE];
} kaizen_rect2i;

#define KAIZEN_VECTOR3_SIZE (sizeof(real_t) * 3)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_VECTOR3_SIZE];
} kaizen_vector3;

#define KAIZEN_VECTOR3I_SIZE (sizeof(int32_t) * 3)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_VECTOR3I_SIZE];
} kaizen_vector3i;

#define KAIZEN_TRANSFORM2D_SIZE (sizeof(real_t) * 6)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_TRANSFORM2D_SIZE];
} kaizen_transform2d;

#define KAIZEN_VECTOR4_SIZE (sizeof(real_t) * 4)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_VECTOR4_SIZE];
} kaizen_vector4;

#define KAIZEN_VECTOR4I_SIZE (sizeof(int32_t) * 4)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_VECTOR4I_SIZE];
} kaizen_vector4i;

#define KAIZEN_PLANE_SIZE (sizeof(real_t) * 4)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_PLANE_SIZE];
} kaizen_plane;

#define KAIZEN_QUATERNION_SIZE (sizeof(real_t) * 4)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_QUATERNION_SIZE];
} kaizen_quaternion;

#define KAIZEN_AABB_SIZE (sizeof(real_t) * 6)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_AABB_SIZE];
} kaizen_aabb;

#define KAIZEN_BASIS_SIZE (sizeof(real_t) * 9)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_BASIS_SIZE];
} kaizen_basis;

#define KAIZEN_TRANSFORM3D_SIZE (sizeof(real_t) * 12)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_TRANSFORM3D_SIZE];
} kaizen_transform3d;

#define KAIZEN_PROJECTION_SIZE (sizeof(real_t) * 4 * 4)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_PROJECTION_SIZE];
} kaizen_projection;

// Colors should always use 32-bit floats, so don't use real_t here.
#define KAIZEN_COLOR_SIZE (sizeof(float) * 4)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_COLOR_SIZE];
} kaizen_color;

#define KAIZEN_NODE_PATH_SIZE sizeof(void *)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_NODE_PATH_SIZE];
} kaizen_node_path;

#define KAIZEN_RID_SIZE sizeof(uint64_t)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_RID_SIZE];
} kaizen_rid;

// Alignment hardcoded in `core/variant/callable.h`.
#define KAIZEN_CALLABLE_SIZE (16)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_CALLABLE_SIZE];
} kaizen_callable;

// Alignment hardcoded in `core/variant/callable.h`.
#define KAIZEN_SIGNAL_SIZE (16)

typedef struct {
	uint8_t _dont_touch_that[KAIZEN_SIGNAL_SIZE];
} kaizen_signal;

#ifdef __cplusplus
}
#endif
