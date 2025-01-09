
cmake_minimum_required(VERSION 3.10)
project(CG_Lab_5)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include(FetchContent)

FetchContent_Declare(
        sfml
        GIT_REPOSITORY "https://github.com/SFML/SFML.git"
        GIT_TAG 69ea0cd863aed1d4092b970b676924a716ff718b
)

set(SFML_BUILD_AUDIO FALSE)
set(SFML_BUILD_NETWORK FALSE)
set(BUILD_SHARED_LIBS TRUE)
set(SFML_USE_STATIC_STD_LIBS FALSE)

FetchContent_MakeAvailable(sfml)

find_package(OpenGL REQUIRED)

add_executable(cg_lab_5 lab5.cpp)

target_include_directories(cg_lab_5 PRIVATE ${sfml_SOURCE_DIR}/include)

target_link_libraries(cg_lab_5 PRIVATE sfml-system sfml-window sfml-graphics ${OPENGL_LIBRARIES})

# Подтвердите правильный путь к выходной директории SFML
add_custom_command(TARGET cg_lab_5 POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${sfml_BINARY_DIR}/bin $<TARGET_FILE_DIR:cg_lab_5>)