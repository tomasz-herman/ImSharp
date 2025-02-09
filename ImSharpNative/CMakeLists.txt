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

find_package(imgui CONFIG REQUIRED)
find_package(implot CONFIG REQUIRED)
find_package(unofficial-imguizmo CONFIG REQUIRED)
find_package(glad CONFIG REQUIRED)
find_package(glfw3 CONFIG REQUIRED)
find_package(unofficial-imguizmo CONFIG REQUIRED)

set (LIBRARIES
        imgui::imgui
        implot::implot
        unofficial::imguizmo::imguizmo
        glfw3
        glad::glad
)

add_library(${PROJECT_NAME} SHARED Library.h Library.cpp)
target_link_libraries(${PROJECT_NAME} PRIVATE ${LIBRARIES})
