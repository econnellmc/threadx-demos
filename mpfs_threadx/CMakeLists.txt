cmake_minimum_required(VERSION 3.27.1)
project(threadx_hello_world LANGUAGES C CXX ASM)

set(THREADX_ARCH "risc-v64")
set(THREADX_TOOLCHAIN "gnu")

# Add ThreadX as a subdirectory
add_subdirectory(${CMAKE_SOURCE_DIR}/../threadx ${CMAKE_BINARY_DIR}/threadx)

# Add the source files
add_executable(mpfs_threadx
    ${CMAKE_CURRENT_SOURCE_DIR}/src/e51.c
    ${CMAKE_CURRENT_SOURCE_DIR}/src/u54_1.c
    ${CMAKE_CURRENT_SOURCE_DIR}/src/u54_2.c
    ${CMAKE_CURRENT_SOURCE_DIR}/src/u54_3.c
    ${CMAKE_CURRENT_SOURCE_DIR}/src/u54_4.c
)

# Add the platform source files using target_sources
target_sources(mpfs_threadx PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/nwc/mss_ddr.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/nwc/mss_ddr_debug.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/nwc/mss_ddr_test_pattern.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/nwc/mss_io.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/nwc/mss_pll.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/nwc/mss_nwc_init.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/nwc/mss_sgmii.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_beu.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_clint.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_h2f.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_irq_handler_stubs.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_l2_cache.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_mpu.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_mtrap.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_peripherals.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_plic.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_pmp.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_util.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/startup_gcc/newlib_stubs.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/startup_gcc/system_startup.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/startup_gcc/mss_entry.S
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/startup_gcc/mss_utils.S
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/mpfs_hal/common/mss_peripherals.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/drivers/mss/mss_mmuart/mss_uart.c
    ${CMAKE_CURRENT_SOURCE_DIR}/platform/drivers/mss/mss_gpio/mss_gpio.c
    ${CMAKE_CURRENT_SOURCE_DIR}/inc/uart_mapping.c
)

target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR})
target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR}/inc)
target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR}/platform)
target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR}/../boards/discovery-kit/platform_config)
target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR}/../boards/discovery-kit)
target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR}/../threadx/common/inc)
target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR}/..)
target_include_directories(mpfs_threadx PRIVATE ${CMAKE_SOURCE_DIR}/../threadx/ports/risc-v64/gnu/inc)


# Link ThreadX to the executable
target_link_libraries(mpfs_threadx threadx)

# Set compile options
target_compile_options(mpfs_threadx PRIVATE -std=gnu11 -nostartfiles -Wall -Wextra -g)

# Set compile options for threadx
target_compile_options(threadx PRIVATE -g)

# Specify the linker script
set(LINKER_SCRIPT ${CMAKE_SOURCE_DIR}/src/linker_script.ld)
target_link_options(mpfs_threadx PRIVATE LINKER:--script=${LINKER_SCRIPT} -nostartfiles -Wl,-Map=mpfs_threadx-mine.map)

target_compile_definitions(mpfs_threadx PRIVATE USING_THREADX NDEBUG ICICLE_KIT_ES)
