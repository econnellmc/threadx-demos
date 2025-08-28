/*******************************************************************************
 * Copyright 2019-2021 Microchip FPGA Embedded Systems Solutions.
 *
 * SPDX-License-Identifier: MIT
 *
 * Application code running on e51
 */

#include <stdio.h>
#include <string.h>
#include "mpfs_hal/mss_hal.h"
#include "drivers/mss/mss_mmuart/mss_uart.h"
#include "drivers/mss/mss_timer/mss_timer.h"
#include "inc/uart_mapping.h"

volatile uint32_t count_sw_ints_h4 = 0U;
extern struct mss_uart_instance *p_uartmap_e51;

const uint8_t g_info_string[] =
    " \r\n\r\n------------------------------------\
---------------------------------\r\n\r\n\
 ThreadX application demo....look into the UART1.\r\n\r\n--------------------------------\
-------------------------------------\r\n";

/* Main function for the hart0(e51 processor).
 * Application code running on hart0 is placed here
 *
 *  hart0 raises a soft interrupt to wake up hart1 and then goes into
 * WFI loop.
 */
void e51(void)
{

	(void)mss_config_clk_rst(MSS_PERIPH_MMUART0, (uint8_t)MPFS_HAL_FIRST_HART, PERIPHERAL_ON);
	(void)mss_config_clk_rst(MSS_PERIPH_GPIO2, (uint8_t)MPFS_HAL_FIRST_HART, PERIPHERAL_ON);

	/* Clear pending software interrupt in case there was any.
	 * Enable only the software interrupt so that the E51 core can bring this
	 * core out of WFI by raising a software interrupt. */

#if (IMAGE_LOADED_BY_BOOTLOADER == 0)

	clear_soft_interrupt();
	set_csr(mie, MIP_MSIP);

	MSS_UART_init(p_uartmap_e51,
		      MSS_UART_115200_BAUD,
		      MSS_UART_DATA_8_BITS | MSS_UART_NO_PARITY | MSS_UART_ONE_STOP_BIT);

	MSS_UART_polled_tx_string(p_uartmap_e51, g_info_string);

	/* Raise software interrupt to wake hart 4 */
	raise_soft_interrupt(4U);

#endif

	while (1)
		;
}

/* hart1 Software interrupt handler */
void Software_h4_IRQHandler(void)
{
	uint64_t hart_id = read_csr(mhartid);
	count_sw_ints_h4++;
}
