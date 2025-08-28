/*******************************************************************************
 * Copyright 2023 Microchip FPGA Embedded Systems Solutions.
 *
 * SPDX-License-Identifier: MIT
 *
 * Application code running on U54_1
 *
 */

#include <stdio.h>
#include <string.h>
#include "mpfs_hal/mss_hal.h"
#include "drivers/mss/mss_mmuart/mss_uart.h"

volatile uint32_t count_sw_ints_h1 = 0U;

/* Main function for the hart4(U54_1 processor).
 * Application code running on hart4 is placed here
 */

void u54_1(void)
{
	uint64_t hartid = read_csr(mhartid);
	volatile uint32_t icount = 0U;

	/* Clear pending software interrupt in case there was any.
	   Enable only the software interrupt so that the E51 core can bring this
	   core out of WFI by raising a software interrupt. */

	clear_soft_interrupt();
	set_csr(mie, MIP_MSIP);
#if (IMAGE_LOADED_BY_BOOTLOADER == 0)
	/* Put this hart in WFI */

	do
	{
		__asm("wfi");
	} while (0 == (read_csr(mip) & MIP_MSIP));
#endif
	/* The hart is out of WFI, clear the SW interrupt. Here onwards application
	 * can enable and use any interrupts as required */

	clear_soft_interrupt();

	__enable_irq();

	while (1U)
	{
		icount++;
		if (0x100000U == icount)
		{
			icount = 0U;
		}
	}

	/* never return */
}

/* hart4 Software interrupt handler */

void Software_h1_IRQHandler(void)
{
	uint64_t hart_id = read_csr(mhartid);
	count_sw_ints_h1++;
}
