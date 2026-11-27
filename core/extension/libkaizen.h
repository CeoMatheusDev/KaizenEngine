/**************************************************************************/
/*  libkaizen.h                                                           */
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

#pragma once

#include "core/extension/gdextension_interface.gen.h"

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

// Export macros for DLL visibility
#if defined(_MSC_VER) || defined(__MINGW32__)
#define LIBKAIZEN_API __declspec(dllexport)
#elif defined(__GNUC__) || defined(__clang__)
#define LIBKAIZEN_API __attribute__((visibility("default")))
#else
#define LIBKAIZEN_API
#endif

/**
 * @name libkaizen_create_kaizen_instance
 * @since 4.6
 *
 * Creates a new Kaizen instance.
 *
 * @param p_argc The number of command line arguments.
 * @param p_argv The C-style array of command line arguments.
 * @param p_init_func GDExtension initialization function of the host application.
 *
 * @return A pointer to created \ref KaizenInstance GDExtension object or nullptr if there was an error.
 */
LIBKAIZEN_API GDExtensionObjectPtr libkaizen_create_kaizen_instance(int p_argc, char *p_argv[], GDExtensionInitializationFunction p_init_func);

/**
 * @name libkaizen_destroy_kaizen_instance
 * @since 4.6
 *
 * Destroys an existing Kaizen instance.
 *
 * @param p_kaizen_instance The reference to the KaizenInstance object to destroy.
 *
 */
LIBKAIZEN_API void libkaizen_destroy_kaizen_instance(GDExtensionObjectPtr p_kaizen_instance);

#ifdef __cplusplus
}
#endif // __cplusplus
