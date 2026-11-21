cmake_minimum_required(VERSION 3.21)
project(ImSharpNative LANGUAGES C CXX)

if (CMAKE_VERSION VERSION_GREATER_EQUAL "3.24.0")
    cmake_policy(SET CMP0135 NEW)
endif()
if (POLICY CMP0169)
    cmake_policy(SET CMP0169 OLD)
endif()

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# ==============================================================================
# Product feature toggles (all ImGui family products enabled by default)
# ==============================================================================
option(IMSHARP_ENABLE_IMPLOT          "Include ImPlot (2D plotting)"                     ON)
option(IMSHARP_ENABLE_IMPLOT3D        "Include ImPlot3D (3D plotting)"                   ON)
option(IMSHARP_ENABLE_IMNODES         "Include imnodes (Nelarius node editor)"           ON)
option(IMSHARP_ENABLE_IMNODES_R       "Include ImNodes (rokups node editor)"             ON)
option(IMSHARP_ENABLE_IMNODES_EDITOR  "Include imgui-node-editor (thedmd node editor)"   ON)
option(IMSHARP_ENABLE_IMGUIZMO        "Include ImGuizmo (3D transform gizmo)"            ON)
option(IMSHARP_ENABLE_IMGUIZMO_QUAT   "Include imGuIZMO.quat (3D quaternion gizmo)"      ON)
option(IMSHARP_ENABLE_IMCTE           "Include ImGuiColorTextEdit (syntax code editor)"  ON)

# ==============================================================================
# Backend feature toggles
# ==============================================================================
option(IMSHARP_BACKEND_GLFW           "Include Dear ImGui GLFW backend"                  ON)
option(IMSHARP_BACKEND_OPENGL3        "Include Dear ImGui OpenGL3 backend"               ON)
option(IMSHARP_BACKEND_OPENGL2        "Include Dear ImGui OpenGL2 backend"               ON)
option(IMSHARP_BACKEND_NULL           "Include Dear ImGui Null (headless) backend"       ON)
option(IMSHARP_BACKEND_VULKAN         "Include Dear ImGui Vulkan backend (if headers found)" ON)
option(IMSHARP_BACKEND_SDL2           "Include Dear ImGui SDL2 + SDLRenderer2 backends (if SDL2 found)" ON)
option(IMSHARP_BACKEND_SDL3           "Include Dear ImGui SDL3 + SDLRenderer3 + SDLGPU3 backends (if SDL3 found)" ON)

# ==============================================================================
# Pinned upstream dependency versions (update these to upgrade ImGui & friends)
# ==============================================================================
set(CIMGUI_GIT_TAG           "1.93.1WIPdock"                            CACHE STRING "cimgui git tag/commit (Dear ImGui 1.93 docking + multi-viewports)")
set(CIMPLOT_GIT_TAG          "v1.1"                                     CACHE STRING "cimplot git tag/commit (ImPlot 1.1)")
set(CIMPLOT3D_GIT_TAG        "c7b527f318c1e18cfe4c05b3cd666ce44175779e" CACHE STRING "cimplot3d git tag/commit (ImPlot3D 0.5)")
set(CIMNODES_GIT_TAG         "712f574082945810dc2f79e022995c86b3b128b9" CACHE STRING "cimnodes git tag/commit")
set(CIMNODES_R_GIT_TAG       "9bf698dfd0c5a686734e9938f07296d8436b2e83" CACHE STRING "cimnodes_r git tag/commit")
set(CIMNODES_EDITOR_GIT_TAG  "917728f58869efafb67386099a1c77e153284f49" CACHE STRING "cimnodes_editor git tag/commit")
set(CIMGUIZMO_GIT_TAG        "566ff53a4ae6ab148312ffc41eef7ba03b312560" CACHE STRING "cimguizmo git tag/commit")
set(CIMGUIZMO_QUAT_GIT_TAG   "91499ef65938d050c4a87150284e2b085db003dd" CACHE STRING "cimguizmo_quat git tag/commit")
set(CIMCTE_GIT_TAG           "dbf2abe4841b9dd8a3f1f1864038ac9d60155c5e" CACHE STRING "cimCTE git tag/commit (ImGuiColorTextEdit)")
set(GLFW_HEADERS_TAG         "3.4"                                      CACHE STRING "GLFW git tag for headers (matches OpenTK.redist.glfw)")
set(GLFW_OPENTK_VERSION      "3.4.0.44"                                 CACHE STRING "OpenTK.redist.glfw NuGet package version (matches OpenTK 4.9.4)")

include(FetchContent)

# Fetch OpenTK's prebuilt GLFW runtime library so ImSharpNative links against the same GLFW as OpenTK
FetchContent_Declare(
    OpenTK_Glfw
    URL https://www.nuget.org/api/v2/package/OpenTK.redist.glfw/${GLFW_OPENTK_VERSION}
    SOURCE_DIR ${CMAKE_BINARY_DIR}/glfw_nuget
)

# Fetch GLFW headers only (no build needed)
FetchContent_Declare(
    glfw_headers
    GIT_REPOSITORY https://github.com/glfw/glfw.git
    GIT_TAG ${GLFW_HEADERS_TAG}
    GIT_SHALLOW TRUE
)

# Core Dear ImGui (cimgui)
FetchContent_Declare(
    cimgui
    GIT_REPOSITORY https://github.com/cimgui/cimgui.git
    GIT_TAG ${CIMGUI_GIT_TAG}
    GIT_SHALLOW TRUE
    GIT_SUBMODULES_RECURSE TRUE
)

FetchContent_Populate(OpenTK_Glfw)
FetchContent_Populate(glfw_headers)
FetchContent_Populate(cimgui)

set(IMGUI_DIR ${cimgui_SOURCE_DIR}/imgui)

set(ALL_SOURCES
    BackendImpl.h
    BackendImpl.cpp
    ${IMGUI_DIR}/imgui.cpp
    ${IMGUI_DIR}/imgui_draw.cpp
    ${IMGUI_DIR}/imgui_demo.cpp
    ${IMGUI_DIR}/imgui_tables.cpp
    ${IMGUI_DIR}/imgui_widgets.cpp
    ${cimgui_SOURCE_DIR}/cimgui.cpp
)

set(ALL_INCLUDES
    ${glfw_headers_SOURCE_DIR}/include
    ${IMGUI_DIR}
    ${IMGUI_DIR}/backends
    ${cimgui_SOURCE_DIR}
)

set(BACKEND_LINK_LIBS "")
set(BACKEND_COMPILE_DEFS "")

if (IMSHARP_BACKEND_GLFW)
    list(APPEND ALL_SOURCES ${IMGUI_DIR}/backends/imgui_impl_glfw.cpp)
endif()

if (IMSHARP_BACKEND_OPENGL3)
    list(APPEND ALL_SOURCES ${IMGUI_DIR}/backends/imgui_impl_opengl3.cpp)
endif()

if (IMSHARP_BACKEND_OPENGL2)
    list(APPEND ALL_SOURCES ${IMGUI_DIR}/backends/imgui_impl_opengl2.cpp)
    if (UNIX AND NOT APPLE)
        find_package(OpenGL QUIET)
        if (OpenGL_FOUND)
            list(APPEND BACKEND_LINK_LIBS OpenGL::GL)
        else()
            list(APPEND BACKEND_LINK_LIBS GL)
        endif()
    endif()
endif()

if (IMSHARP_BACKEND_NULL)
    list(APPEND ALL_SOURCES ${IMGUI_DIR}/backends/imgui_impl_null.cpp)
endif()

if (IMSHARP_BACKEND_VULKAN)
    find_path(VULKAN_INCLUDE_DIR vulkan/vulkan.h)
    if (VULKAN_INCLUDE_DIR)
        message(STATUS "ImSharpNative: Enabling Vulkan backend (${VULKAN_INCLUDE_DIR})")
        list(APPEND ALL_SOURCES ${IMGUI_DIR}/backends/imgui_impl_vulkan.cpp)
        list(APPEND ALL_INCLUDES ${VULKAN_INCLUDE_DIR})
        list(APPEND BACKEND_COMPILE_DEFS VK_NO_PROTOTYPES IMGUI_IMPL_VULKAN_NO_PROTOTYPES)
    else()
        message(STATUS "ImSharpNative: Vulkan headers not found; skipping Vulkan backend")
    endif()
endif()

if (IMSHARP_BACKEND_SDL2)
    find_package(SDL2 QUIET)
    if (SDL2_FOUND)
        message(STATUS "ImSharpNative: Enabling SDL2 + SDLRenderer2 backends")
        list(APPEND ALL_SOURCES
            ${IMGUI_DIR}/backends/imgui_impl_sdl2.cpp
            ${IMGUI_DIR}/backends/imgui_impl_sdlrenderer2.cpp
        )
        if (TARGET SDL2::SDL2)
            list(APPEND BACKEND_LINK_LIBS SDL2::SDL2)
        else()
            list(APPEND ALL_INCLUDES ${SDL2_INCLUDE_DIRS})
            list(APPEND BACKEND_LINK_LIBS ${SDL2_LIBRARIES})
        endif()
    else()
        message(STATUS "ImSharpNative: SDL2 not found; skipping SDL2 backends")
    endif()
endif()

if (IMSHARP_BACKEND_SDL3)
    find_package(SDL3 QUIET)
    if (SDL3_FOUND)
        message(STATUS "ImSharpNative: Enabling SDL3 + SDLRenderer3 + SDLGPU3 backends")
        list(APPEND ALL_SOURCES
            ${IMGUI_DIR}/backends/imgui_impl_sdl3.cpp
            ${IMGUI_DIR}/backends/imgui_impl_sdlrenderer3.cpp
            ${IMGUI_DIR}/backends/imgui_impl_sdlgpu3.cpp
        )
        list(APPEND BACKEND_LINK_LIBS SDL3::SDL3)
    else()
        message(STATUS "ImSharpNative: SDL3 not found; skipping SDL3 backends")
    endif()
endif()

if (WIN32)
    list(APPEND ALL_SOURCES
        ${IMGUI_DIR}/backends/imgui_impl_win32.cpp
        ${IMGUI_DIR}/backends/imgui_impl_dx9.cpp
        ${IMGUI_DIR}/backends/imgui_impl_dx10.cpp
        ${IMGUI_DIR}/backends/imgui_impl_dx11.cpp
        ${IMGUI_DIR}/backends/imgui_impl_dx12.cpp
    )
    list(APPEND BACKEND_LINK_LIBS d3d9 d3d10 d3d11 d3d12 dxgi d3dcompiler)
endif()

# ImPlot (2D plotting)
if (IMSHARP_ENABLE_IMPLOT)
    FetchContent_Declare(
        cimplot
        GIT_REPOSITORY https://github.com/cimgui/cimplot.git
        GIT_TAG ${CIMPLOT_GIT_TAG}
        GIT_SHALLOW TRUE
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimplot)
    list(APPEND ALL_SOURCES
        ${cimplot_SOURCE_DIR}/implot/implot.cpp
        ${cimplot_SOURCE_DIR}/implot/implot_demo.cpp
        ${cimplot_SOURCE_DIR}/implot/implot_items.cpp
        ${cimplot_SOURCE_DIR}/cimplot.cpp
    )
    list(APPEND ALL_INCLUDES
        ${cimplot_SOURCE_DIR}/implot
        ${cimplot_SOURCE_DIR}
    )
endif()

# ImPlot3D (3D plotting)
if (IMSHARP_ENABLE_IMPLOT3D)
    FetchContent_Declare(
        cimplot3d
        GIT_REPOSITORY https://github.com/cimgui/cimplot3d.git
        GIT_TAG ${CIMPLOT3D_GIT_TAG}
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimplot3d)
    file(GLOB IMPLOT3D_CPP CONFIGURE_DEPENDS "${cimplot3d_SOURCE_DIR}/implot3d/*.cpp")
    list(APPEND ALL_SOURCES
        ${IMPLOT3D_CPP}
        ${cimplot3d_SOURCE_DIR}/cimplot3d.cpp
    )
    list(APPEND ALL_INCLUDES
        ${cimplot3d_SOURCE_DIR}/implot3d
        ${cimplot3d_SOURCE_DIR}
    )
endif()

# imnodes (Nelarius)
if (IMSHARP_ENABLE_IMNODES)
    FetchContent_Declare(
        cimnodes
        GIT_REPOSITORY https://github.com/cimgui/cimnodes.git
        GIT_TAG ${CIMNODES_GIT_TAG}
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimnodes)
    list(APPEND ALL_SOURCES
        ${cimnodes_SOURCE_DIR}/imnodes/imnodes.cpp
        ${cimnodes_SOURCE_DIR}/cimnodes.cpp
    )
    list(APPEND ALL_INCLUDES
        ${cimnodes_SOURCE_DIR}/imnodes
        ${cimnodes_SOURCE_DIR}
    )
endif()

# ImNodes (rokups)
if (IMSHARP_ENABLE_IMNODES_R)
    FetchContent_Declare(
        cimnodes_r
        GIT_REPOSITORY https://github.com/cimgui/cimnodes_r.git
        GIT_TAG ${CIMNODES_R_GIT_TAG}
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimnodes_r)
    list(APPEND ALL_SOURCES
        ${cimnodes_r_SOURCE_DIR}/ImNodes/ImNodes.cpp
        ${cimnodes_r_SOURCE_DIR}/ImNodes/ImNodesEz.cpp
        ${cimnodes_r_SOURCE_DIR}/cimnodes_r.cpp
    )
    list(APPEND ALL_INCLUDES
        ${cimnodes_r_SOURCE_DIR}/ImNodes
        ${cimnodes_r_SOURCE_DIR}
    )
endif()

# imgui-node-editor (thedmd)
if (IMSHARP_ENABLE_IMNODES_EDITOR)
    FetchContent_Declare(
        cimnodes_editor
        GIT_REPOSITORY https://github.com/cimgui/cimnodes_editor.git
        GIT_TAG ${CIMNODES_EDITOR_GIT_TAG}
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimnodes_editor)
    file(GLOB IMNODES_EDITOR_CPP CONFIGURE_DEPENDS "${cimnodes_editor_SOURCE_DIR}/imgui-node-editor/*.cpp")
    list(APPEND ALL_SOURCES
        ${IMNODES_EDITOR_CPP}
        ${cimnodes_editor_SOURCE_DIR}/cimnodes_editor.cpp
    )
    list(APPEND ALL_INCLUDES
        ${cimnodes_editor_SOURCE_DIR}/imgui-node-editor
        ${cimnodes_editor_SOURCE_DIR}
    )
endif()

# ImGuizmo (CedricGuillemet)
if (IMSHARP_ENABLE_IMGUIZMO)
    FetchContent_Declare(
        cimguizmo
        GIT_REPOSITORY https://github.com/cimgui/cimguizmo.git
        GIT_TAG ${CIMGUIZMO_GIT_TAG}
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimguizmo)
    set(IMGUIZMO_DIR ${cimguizmo_SOURCE_DIR}/ImGuizmo/src)
    list(APPEND ALL_SOURCES
        ${IMGUIZMO_DIR}/ImGuizmo.cpp
        ${IMGUIZMO_DIR}/GraphEditor.cpp
        ${IMGUIZMO_DIR}/ImCurveEdit.cpp
        ${IMGUIZMO_DIR}/ImGradient.cpp
        ${IMGUIZMO_DIR}/ImSequencer.cpp
        ${cimguizmo_SOURCE_DIR}/cimguizmo.cpp
    )
    list(APPEND ALL_INCLUDES
        ${IMGUIZMO_DIR}
        ${cimguizmo_SOURCE_DIR}
    )
endif()

# imGuIZMO.quat (BrutPitt)
if (IMSHARP_ENABLE_IMGUIZMO_QUAT)
    FetchContent_Declare(
        cimguizmo_quat
        GIT_REPOSITORY https://github.com/cimgui/cimguizmo_quat.git
        GIT_TAG ${CIMGUIZMO_QUAT_GIT_TAG}
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimguizmo_quat)
    list(APPEND ALL_SOURCES
        ${cimguizmo_quat_SOURCE_DIR}/imGuIZMO.quat/imguizmo_quat/imGuIZMOquat.cpp
        ${cimguizmo_quat_SOURCE_DIR}/cimguizmo_quat.cpp
    )
    list(APPEND ALL_INCLUDES
        ${cimguizmo_quat_SOURCE_DIR}/imGuIZMO.quat/imguizmo_quat
        ${cimguizmo_quat_SOURCE_DIR}
    )
endif()

# ImGuiColorTextEdit (cimCTE)
if (IMSHARP_ENABLE_IMCTE)
    FetchContent_Declare(
        cimcte
        GIT_REPOSITORY https://github.com/cimgui/cimCTE.git
        GIT_TAG ${CIMCTE_GIT_TAG}
        GIT_SUBMODULES_RECURSE TRUE
    )
    FetchContent_Populate(cimcte)
    list(APPEND ALL_SOURCES
        ${cimcte_SOURCE_DIR}/ImGuiColorTextEdit/TextEditor.cpp
        ${cimcte_SOURCE_DIR}/ImGuiColorTextEdit/TextDiff.cpp
        ${cimcte_SOURCE_DIR}/ImGuiColorTextEdit/example/dejavu.cpp
        ${cimcte_SOURCE_DIR}/ImGuiColorTextEdit/example/notosans.cpp
        ${cimcte_SOURCE_DIR}/ImGuiColorTextEdit/extras/TrieAutoComplete.cpp
        ${cimcte_SOURCE_DIR}/cimCTE.cpp
    )
    list(APPEND ALL_INCLUDES
        ${cimcte_SOURCE_DIR}/ImGuiColorTextEdit
        ${cimcte_SOURCE_DIR}
    )
endif()

add_library(${PROJECT_NAME} SHARED ${ALL_SOURCES})

# ==============================================================================
# Platform-specific GLFW & OpenGL linkage
# ==============================================================================
if (WIN32)
    set(GLFW_LIB "${CMAKE_BINARY_DIR}/glfw_nuget/runtimes/win-x64/native/glfw3.dll")
    target_compile_definitions(${PROJECT_NAME} PRIVATE
        IMGUI_API=__declspec\(dllexport\)
        IMPLOT_API=__declspec\(dllexport\)
        IMPLOT3D_API=__declspec\(dllexport\)
        "IMGUI_IMPL_API=extern \"C\" __declspec(dllexport)"
    )
    target_link_libraries(${PROJECT_NAME} PRIVATE opengl32 imm32)
elseif (APPLE)
    set(GLFW_LIB "${CMAKE_BINARY_DIR}/glfw_nuget/runtimes/osx-x64/native/libglfw.3.dylib")
    add_library(glfw3 SHARED IMPORTED)
    set_target_properties(glfw3 PROPERTIES IMPORTED_LOCATION "${GLFW_LIB}")
    target_compile_definitions(${PROJECT_NAME} PRIVATE
        "IMGUI_IMPL_API=extern \"C\""
    )
    target_link_libraries(${PROJECT_NAME} PRIVATE glfw3 "-framework OpenGL")
elseif (UNIX)
    set(GLFW_LIB "${CMAKE_BINARY_DIR}/glfw_nuget/runtimes/linux-x64/native/libglfw.so.3")
    add_library(glfw3 SHARED IMPORTED)
    set_target_properties(glfw3 PROPERTIES
        IMPORTED_LOCATION "${GLFW_LIB}"
        IMPORTED_SONAME "libglfw.so.3"
    )
    target_compile_definitions(${PROJECT_NAME} PRIVATE
        "IMGUI_IMPL_API=extern \"C\""
    )
    target_link_libraries(${PROJECT_NAME} PRIVATE glfw3 dl)
    set_target_properties(${PROJECT_NAME} PROPERTIES
        BUILD_RPATH "\$ORIGIN:\$ORIGIN/runtimes/linux-x64/native"
        INSTALL_RPATH "\$ORIGIN:\$ORIGIN/runtimes/linux-x64/native"
    )
else()
    message(FATAL_ERROR "Unsupported platform")
endif()

target_compile_definitions(${PROJECT_NAME} PRIVATE
    GLFW_INCLUDE_NONE
    IMGUI_DISABLE_OBSOLETE_FUNCTIONS=1
    CIMGUI_VARGS0
    IMNODES_NAMESPACE=imnodes
    ${BACKEND_COMPILE_DEFS}
)

target_link_libraries(${PROJECT_NAME} PRIVATE ${BACKEND_LINK_LIBS})

target_include_directories(${PROJECT_NAME} PRIVATE ${ALL_INCLUDES})

if (UNIX AND NOT APPLE)
    target_compile_options(${PROJECT_NAME} PRIVATE -fvisibility=default)
    target_link_options(${PROJECT_NAME} PRIVATE -Wl,--no-undefined)
endif()
