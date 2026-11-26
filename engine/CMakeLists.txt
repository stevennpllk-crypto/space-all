cmake_minimum_required(VERSION 3.20)
project(JASSIMEngine VERSION 0.2.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

add_library(jassim_engine
    src/Scene.cpp
    src/DeviceProfile.cpp
)

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
if(BUILD_TESTING)
    add_executable(jassim_engine_smoke tests/engine_smoke.cpp)
    target_link_libraries(jassim_engine_smoke PRIVATE jassim_engine)
    add_test(NAME jassim_engine_smoke COMMAND jassim_engine_smoke)
endif()
