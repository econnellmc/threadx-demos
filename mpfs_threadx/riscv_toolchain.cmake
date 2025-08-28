set(CMAKE_SYSTEM_NAME Generic)
set(CMAKE_SYSTEM_PROCESSOR riscv)

# Specify the cross-compilation tools
set(CMAKE_SHARED_LIBRARY_LINK_C_FLAGS "")
set(CMAKE_SHARED_LIBRARY_LINK_CXX_FLAGS "")

if(${CMAKE_HOST_SYSTEM_NAME} STREQUAL "Linux")
	set(CMAKE_MAKE_PROGRAM ${CMAKE_SOURCE_DIR}/../ninja/ninja CACHE FILEPATH "")
elseif(${CMAKE_HOST_SYSTEM_NAME} STREQUAL "Windows")
	set(CMAKE_MAKE_PROGRAM ${CMAKE_SOURCE_DIR}/../ninja/ninja.exe CACHE FILEPATH "")
endif()
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)
set(CMAKE_GENERATOR "Ninja" CACHE INTERNAL "" FORCE)

# variable that is used for the toolchain
set(riscv_toolchain_base ${CMAKE_SOURCE_DIR}/../xpack-riscv-none-elf-gcc-14.2.0-3)

# here is the target environment located
set(CMAKE_FIND_ROOT_PATH ${riscv_toolchain_base})

set(riscv_toolchain_bin_path ${riscv_toolchain_base}/bin)
set(riscv_toolchain_prefix riscv-none-elf)

set(CMAKE_LIBRARY_ARCHITECTURE ${riscv_toolchain_bin_path})

if(UNIX)
    set(executable_extension)
elseif(WIN32)
    set(executable_extension .exe)
endif()

# which compilers to use for C and C++
set(CMAKE_C_COMPILER ${riscv_toolchain_bin_path}/${riscv_toolchain_prefix}-gcc${executable_extension})
set(CMAKE_CXX_COMPILER ${riscv_toolchain_bin_path}/${riscv_toolchain_prefix}-g++${executable_extension})
set(CMAKE_AR ${riscv_toolchain_bin_path}/${riscv_toolchain_prefix}-ar${executable_extension})
set(CMAKE_ASM_COMPILER ${riscv_toolchain_bin_path}/${riscv_toolchain_prefix}-gcc${executable_extension})

# We must set the OBJCOPY setting into cache so that it's available to the
# whole project. Otherwise, this does not get set into the CACHE and therefore
# the build doesn't know what the OBJCOPY filepath is
set(CMAKE_OBJCOPY ${riscv_toolchain_bin_path}/${riscv_toolchain_prefix}-objcopy${executable_extension} CACHE FILEPATH "The toolchain objcopy command " FORCE )
set(CMAKE_OBJDUMP ${riscv_toolchain_bin_path}/${riscv_toolchain_prefix}-objdump${executable_extension} CACHE FILEPATH "The toolchain objdump command " FORCE )
set(CMAKE_SIZE ${riscv_toolchain_bin_path}/${riscv_toolchain_prefix}-size${executable_extension} CACHE FILEPATH "The toolchain size command " FORCE )

set(CMAKE_ASM_SOURCE_FILE_EXTENSIONS S)
# adjust the default behaviour of the FIND_XXX() commands:
# search headers and libraries in the target environment, search
# programs in the host environment
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_PACKAGE ONLY)

# Specify the compiler flags
set(CMAKE_C_FLAGS "-march=rv64imafdc_zicsr_zifencei -mabi=lp64d -mcmodel=medany")
set(CMAKE_CXX_FLAGS "-march=rv64imafdc_zicsr_zifencei -mabi=lp64d -mcmodel=medany")
set(CMAKE_ASM_FLAGS "-march=rv64imafdc_zicsr_zifencei -mabi=lp64d -mcmodel=medany")

set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -O2 -fno-builtin-printf")

set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -msmall-data-limit=8")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -mstrict-align")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -mno-save-restore")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -std=gnu11")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wstrict-prototypes")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wbad-function-cast")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fmessage-length=0")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fsigned-char")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -ffunction-sections")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fdata-sections")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fcommon")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -fstack-protector-strong")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -mexplicit-relocs")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wl,-allow-multiple-definition")

set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -msmall-data-limit=8")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -mstrict-align")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -mno-save-restore")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fmessage-length=0")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fsigned-char")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -ffunction-sections")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fdata-sections")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fcommon")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fstack-protector-strong")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++17")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fabi-version=13")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-exceptions")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-rtti")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-use-cxa-atexit")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -fno-threadsafe-statics")
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wl,-allow-multiple-definition")

set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -msmall-data-limit=8")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -mstrict-align")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -mno-save-restore")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -fmessage-length=0")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -fsigned-char")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -ffunction-sections")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -fdata-sections")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -fcommon")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -fstack-protector-strong")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -std=gnu11")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -Wstrict-prototypes")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -Wbad-function-cast")
set(CMAKE_ASM_FLAGS "${CMAKE_ASM_FLAGS} -x assembler-with-cpp")

# Optionally, specify the path to the RISC-V toolchain binaries if they are not in the system PATH
# set(CMAKE_FIND_ROOT_PATH /path/to/riscv-toolchain)
# set(CMAKE_PROGRAM_PATH ${CMAKE_FIND_ROOT_PATH}/bin)
# set(CMAKE_C_COMPILER ${CMAKE_PROGRAM_PATH}/riscv-none-elf-gcc)
# set(CMAKE_CXX_COMPILER ${CMAKE_PROGRAM_PATH}/riscv-none-elf-g++)
