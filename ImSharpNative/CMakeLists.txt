cmake_minimum_required(VERSION 3.21)
project(ImSharpNative)

if (CMAKE_VERSION VERSION_GREATER_EQUAL "3.24.0")
    cmake_policy(SET CMP0135 NEW)
endif()

set(CMAKE_CXX_STANDARD 23)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -DGLFW_INCLUDE_NONE")

set(GLFW_OPENTK_VERSION "3.4.0.44")
set(GLFW_OPENTK_DIR "glfw_nuget")

include(FetchContent)
FetchContent_Declare(
        OpenTK_Glfw
        URL https://www.nuget.org/api/v2/package/OpenTK.redist.glfw/${GLFW_OPENTK_VERSION}
        SOURCE_DIR ${CMAKE_BINARY_DIR}/${GLFW_OPENTK_DIR}
)
FetchContent_MakeAvailable(OpenTK_Glfw)

FetchContent_Declare(
        cimgui
        GIT_REPOSITORY https://github.com/cimgui/cimgui.git
        GIT_TAG master
)
FetchContent_MakeAvailable(cimgui)

FetchContent_Declare(
        cimplot
        GIT_REPOSITORY https://github.com/cimgui/cimplot.git
        GIT_TAG master
)
FetchContent_MakeAvailable(cimplot)

FetchContent_Declare(
        cimnodes
        GIT_REPOSITORY https://github.com/cimgui/cimnodes.git
        GIT_TAG master
)
FetchContent_MakeAvailable(cimnodes)

FetchContent_Declare(
        cimguizmo
        GIT_REPOSITORY https://github.com/cimgui/cimguizmo.git
        GIT_TAG master
)
FetchContent_MakeAvailable(cimguizmo)

add_definitions(-DIMNODES_NAMESPACE=imnodes)
set(CIMGUI_SRC ${cimgui_SOURCE_DIR}/cimgui.cpp)
set(CIMPLOT_SRC ${cimplot_SOURCE_DIR}/cimplot.cpp)
set(CIMNODES_SRC ${cimnodes_SOURCE_DIR}/cimnodes.cpp)
set(CIMGUIMO_SRC ${cimguizmo_SOURCE_DIR}/cimguizmo.cpp)

if(WIN32)
    set(GLFW_LIB "${CMAKE_BINARY_DIR}/${GLFW_OPENTK_DIR}/runtimes/win-x64/native/glfw3.dll")
elseif(APPLE)
    set(GLFW_LIB "${CMAKE_BINARY_DIR}/${GLFW_OPENTK_DIR}/runtimes/osx-x64/native/libglfw.3.dylib")
elseif(UNIX)
    set(GLFW_LIB "${CMAKE_BINARY_DIR}/${GLFW_OPENTK_DIR}/runtimes/linux-x64/native/libglfw.so.3")
else()
    message(FATAL_ERROR "Unsupported platform")
endif()

message("Toolchain used: ${CMAKE_TOOLCHAIN_FILE}")

add_library(glfw3 SHARED IMPORTED)
set_target_properties(glfw3 PROPERTIES IMPORTED_LOCATION "${GLFW_LIB}")

find_package(glad CONFIG REQUIRED)
find_package(glfw3 CONFIG REQUIRED)

target_link_libraries(cimplot PRIVATE cimgui)

add_library(${PROJECT_NAME} SHARED
        BackendImpl.h
        BackendImpl.cpp
        ${CIMGUI_SRC}
        ${CIMPLOT_SRC}
        ${CIMNODES_SRC}
        ${CIMGUIMO_SRC})

target_link_libraries(${PROJECT_NAME} PUBLIC glfw3 glad::glad)

target_include_directories(${PROJECT_NAME} PUBLIC ${cimgui_SOURCE_DIR}/imgui/backends)
target_include_directories(${PROJECT_NAME} PUBLIC ${cimgui_SOURCE_DIR}/imgui)

target_include_directories(${PROJECT_NAME} PUBLIC
        ${cimgui_SOURCE_DIR}
        ${cimplot_SOURCE_DIR}
        ${cimnodes_SOURCE_DIR}
        ${cimguizmo_SOURCE_DIR}
)

set_target_properties(${PROJECT_NAME} PROPERTIES POSITION_INDEPENDENT_CODE ON)
