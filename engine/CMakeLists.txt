cmake_minimum_required(VERSION 3.20)
project(JASSIMEngine VERSION 0.12.0 LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

set(JASSIM_ENGINE_SOURCES
    src/Scene.cpp
    src/DeviceProfile.cpp
)

if(ANDROID)
    set(ANDROID_NATIVE_APP_GLUE "${CMAKE_ANDROID_NDK}/sources/android/native_app_glue")
    list(APPEND JASSIM_ENGINE_SOURCES
        src/android/NativeActivity.cpp
        "${ANDROID_NATIVE_APP_GLUE}/android_native_app_glue.c"
    )
    add_library(jassim_engine SHARED ${JASSIM_ENGINE_SOURCES})
    target_include_directories(jassim_engine PRIVATE "${ANDROID_NATIVE_APP_GLUE}")
    target_link_libraries(jassim_engine PRIVATE android log EGL GLESv3)
    target_link_options(jassim_engine PRIVATE "-uANativeActivity_onCreate")
else()
    add_library(jassim_engine ${JASSIM_ENGINE_SOURCES})
endif()

target_include_directories(jassim_engine
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_features(jassim_engine PUBLIC cxx_std_17)

if(MSVC)
    target_compile_options(jassim_engine PRIVATE /W4 /permissive-)
else()
    target_compile_options(jassim_engine PRIVATE -Wall -Wextra -Wpedantic)
endif()

include(CTest)
if(BUILD_TESTING AND NOT ANDROID)
    add_executable(jassim_engine_smoke tests/engine_smoke.cpp)
    target_link_libraries(jassim_engine_smoke PRIVATE jassim_engine)
    add_test(NAME jassim_engine_smoke COMMAND jassim_engine_smoke)
endif()
