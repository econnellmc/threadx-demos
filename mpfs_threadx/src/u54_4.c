/***********************************************************************************
 * Copyright 2019-2021 Microchip FPGA Embedded Systems Solutions.
 *
 * SPDX-License-Identifier: MIT
 * Application code running on U54_4
 *
 * PolarFire SoC MSS TIMER example
 */

#include <stdio.h>
#include <string.h>
#include "mpfs_hal/mss_hal.h"
#include "drivers/mss/mss_timer/mss_timer.h"
#include "drivers/mss/mss_mmuart/mss_uart.h"
#include "drivers/mss/mss_gpio/mss_gpio.h"
#include "threadx/common/inc/tx_api.h"
#include "inc/uart_mapping.h"

/* Sequence of delays */
#define SEQUENCE_LENGTH 5U
#define RX_BUFF_SIZE 64U

uint8_t g_rx_buff[RX_BUFF_SIZE] = {0};
volatile uint8_t g_rx_size = 0;
static uint8_t p_buff[100];
extern mss_uart_instance_t *p_uartmap_u54_1;
/******************************************************************************
 * Instruction message. This message will be transmitted to the UART terminal
 * when the program starts.
 *****************************************************************************/
uint8_t g_message[] =
    "\r\n****************************************************** \
\r\n\r\n      PolarFire SoC MSS TIMER example     \r\n\r\n\
******************************************************\r\n\r\n\
\r\nThis example project demonstrates the use of the PolarFire SoC MSS timer \r\n ";

uint8_t g_menu[] =
    "Choose one of the following Option to observe its corresponding Timer mode. \r\n\n\
Type 0: show this menu\r\n\
Type 1: Configure Timer1 as 32 bit timer in periodic mode (default) \r\n\
Type 2: Configure Timer1 as 32 bit timer in one-shot mode \r\n\
Type 3: configure Timer1 to generate interrupts at non uniform interval using background load API \r\n\r\n\
Type 4: Configure Timer as 64 bit timer in periodic mode \r\n ";

uint8_t g_message2[] =
    "\r\r\nObserve the messages on UART2 terminal .\
     \r\nThe messages are displayed when the timer interrupt occurs \r\n\n";

static const uint32_t g_sequence_delays[SEQUENCE_LENGTH] =
    {
	25000000,
	83000000,
	166000000,
	25000000,
	166000000};

uint8_t timer_config = 0;

typedef enum _timer_menu_options_t
{
	PERIODIC = 0,
	ONE_SHOT,
	TIMER64_PERIODIC,
	BACKGROUND_LOAD
} timer_menu_options_t;

timer_menu_options_t g_current_option = 0;

/* Define the counters used in the demo application...  */

ULONG thread_0_counter;
ULONG thread_1_counter;
ULONG thread_1_messages_sent;
ULONG thread_2_counter;
ULONG thread_2_messages_received;
ULONG thread_3_counter;
ULONG thread_4_counter;
ULONG thread_5_counter;
ULONG thread_6_counter;
ULONG thread_7_counter;

/* Main function for the hart1(U54_4 processor).
 * Application code running on hart1 is placed here.
 */
void u54_4(void)
{
	uint32_t hartid = read_csr(mhartid);
	uint32_t timer_load_value;
	uint8_t p_buff[100];
	clear_soft_interrupt();
	set_csr(mie, MIP_MSIP);

#if (IMAGE_LOADED_BY_BOOTLOADER == 0)
	/*Put this hart into WFI.*/
	do
	{
		__asm("wfi");
	} while (0 == (read_csr(mip) & MIP_MSIP));

	/* The hart is out of WFI, clear the SW interrupt. Hear onwards Application
	 * can enable and use any interrupts as required */
	clear_soft_interrupt();
#endif

	PLIC_init();
	__enable_irq();

	/* Reset the peripherals turn on the clocks */

	mss_config_clk_rst(MSS_PERIPH_MMUART_U54_1, (uint8_t)MPFS_HAL_LAST_HART, PERIPHERAL_ON);
	mss_config_clk_rst(MSS_PERIPH_TIMER, (uint8_t)MPFS_HAL_LAST_HART, PERIPHERAL_ON);

	MSS_UART_init(p_uartmap_u54_1,
		      MSS_UART_115200_BAUD,
		      MSS_UART_DATA_8_BITS | MSS_UART_NO_PARITY | MSS_UART_ONE_STOP_BIT);

	MSS_UART_polled_tx_string(p_uartmap_u54_1, "U54_4 UART \r\n");

	PLIC_SetPriority(TIMER1_PLIC, 2);
	PLIC_SetPriority(TIMER2_PLIC, 2);

	MSS_GPIO_init(GPIO2_LO);
	MSS_GPIO_config(GPIO2_LO, MSS_GPIO_17, MSS_GPIO_OUTPUT_MODE);

	SysTick_Config();

	tx_kernel_enter();

	sprintf(p_buff, "Program End\r\n"); // Ideally, should never reach here
	MSS_UART_polled_tx(p_uartmap_u54_1, p_buff, strlen(p_buff));
	while (1)
		;
}

#define DEMO_STACK_SIZE 2048
#define DEMO_BYTE_POOL_SIZE 20480 * 2
#define DEMO_BLOCK_POOL_SIZE 100
#define DEMO_QUEUE_SIZE 100

/* Define the ThreadX object control blocks...  */

TX_THREAD thread_0;
TX_THREAD thread_1;
TX_THREAD thread_2;
TX_THREAD thread_3;
TX_THREAD thread_4;
TX_THREAD thread_5;
TX_THREAD thread_6;
TX_THREAD thread_7;
TX_QUEUE queue_0;
TX_SEMAPHORE semaphore_0;
TX_MUTEX mutex_0;
TX_EVENT_FLAGS_GROUP event_flags_0;
TX_BYTE_POOL byte_pool_0;
TX_BLOCK_POOL block_pool_0;

/* Define byte pool memory.  */

UCHAR byte_pool_memory[DEMO_BYTE_POOL_SIZE];

/* Define event buffer.  */

#ifdef TX_ENABLE_EVENT_TRACE
UCHAR trace_buffer[0x10000];
#endif

/* Define thread prototypes.  */

void thread_0_entry(ULONG thread_input);
void thread_1_entry(ULONG thread_input);
void thread_2_entry(ULONG thread_input);
void thread_3_and_4_entry(ULONG thread_input);
void thread_5_entry(ULONG thread_input);
void thread_6_and_7_entry(ULONG thread_input);

void tx_application_define(void *first_unused_memory)
{

	CHAR *pointer;

#ifdef TX_ENABLE_EVENT_TRACE
	tx_trace_enable(trace_buffer, sizeof(trace_buffer), 32);
#endif

	/* Create a byte memory pool from which to allocate the thread stacks.  */
	tx_byte_pool_create(&byte_pool_0, "byte pool 0", byte_pool_memory, DEMO_BYTE_POOL_SIZE);

	/* Put system definition stuff in here, e.g. thread creates and other assorted
	   create information.  */

	/* Allocate the stack for thread 0.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	/* Create the main thread.  */
	tx_thread_create(&thread_0, "thread 0", thread_0_entry, 0,
			 pointer, DEMO_STACK_SIZE,
			 1, 1, TX_NO_TIME_SLICE, TX_AUTO_START);

#if 1
	// #ifdef MCHP_DEBUG_ALLDISABLE
	/* Allocate the stack for thread 1.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	/* Create threads 1 and 2. These threads pass information through a ThreadX
	   message queue.  It is also interesting to note that these threads have a time
	   slice.  */
	tx_thread_create(&thread_1, "thread 1", thread_1_entry, 1,
			 pointer, DEMO_STACK_SIZE,
			 16, 16, 4, TX_AUTO_START);

	/* Allocate the stack for thread 2.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	tx_thread_create(&thread_2, "thread 2", thread_2_entry, 2,
			 pointer, DEMO_STACK_SIZE,
			 16, 16, 4, TX_AUTO_START);
#endif

#if 1
	/* Allocate the stack for thread 3.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	/* Create threads 3 and 4.  These threads compete for a ThreadX counting semaphore.
	   An interesting thing here is that both threads share the same instruction area.  */
	tx_thread_create(&thread_3, "thread 3", thread_3_and_4_entry, 3,
			 pointer, DEMO_STACK_SIZE,
			 8, 8, TX_NO_TIME_SLICE, TX_AUTO_START);

	/* Allocate the stack for thread 4.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	tx_thread_create(&thread_4, "thread 4", thread_3_and_4_entry, 4,
			 pointer, DEMO_STACK_SIZE,
			 8, 8, TX_NO_TIME_SLICE, TX_AUTO_START);

	/* Allocate the stack for thread 5.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	/* Create thread 5.  This thread simply pends on an event flag which will be set
	   by thread_0.  */
	tx_thread_create(&thread_5, "thread 5", thread_5_entry, 5,
			 pointer, DEMO_STACK_SIZE,
			 4, 4, TX_NO_TIME_SLICE, TX_AUTO_START);

	// #ifdef MCHP_DEBUG_ALLDISABLE
	/* Allocate the stack for thread 6.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	/* Create threads 6 and 7.  These threads compete for a ThreadX mutex.  */
	tx_thread_create(&thread_6, "thread 6", thread_6_and_7_entry, 6,
			 pointer, DEMO_STACK_SIZE,
			 8, 8, TX_NO_TIME_SLICE, TX_AUTO_START);

	/* Allocate the stack for thread 7.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_STACK_SIZE, TX_NO_WAIT);

	tx_thread_create(&thread_7, "thread 7", thread_6_and_7_entry, 7,
			 pointer, DEMO_STACK_SIZE,
			 8, 8, TX_NO_TIME_SLICE, TX_AUTO_START);
#endif

	/* Allocate the message queue.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_QUEUE_SIZE * sizeof(ULONG), TX_NO_WAIT);

	/* Create the message queue shared by threads 1 and 2.  */
	tx_queue_create(&queue_0, "queue 0", TX_1_ULONG, pointer, DEMO_QUEUE_SIZE * sizeof(ULONG));

	/* Create the semaphore used by threads 3 and 4.  */
	tx_semaphore_create(&semaphore_0, "semaphore 0", 1);

	/* Create the event flags group used by threads 1 and 5.  */
	tx_event_flags_create(&event_flags_0, "event flags 0");

	/* Create the mutex used by thread 6 and 7 without priority inheritance.  */
	tx_mutex_create(&mutex_0, "mutex 0", TX_NO_INHERIT);

	/* Allocate the memory for a small block pool.  */
	tx_byte_allocate(&byte_pool_0, (VOID **)&pointer, DEMO_BLOCK_POOL_SIZE, TX_NO_WAIT);

	/* Create a block memory pool to allocate a message buffer from.  */
	tx_block_pool_create(&block_pool_0, "block pool 0", sizeof(ULONG), pointer, DEMO_BLOCK_POOL_SIZE);

	/* Allocate a block and release the block memory.  */
	tx_block_allocate(&block_pool_0, (VOID **)&pointer, TX_NO_WAIT);

	/* Release the block back to the pool.  */
	tx_block_release(pointer);
}

/* Define the test threads.  */

void thread_0_entry(ULONG thread_input)
{

	UINT status;

	/* This thread simply sits in while-forever-sleep loop.  */
	while (1)
	{

		/* Increment the thread counter.  */
		thread_0_counter++;

		/* Sleep for 10 ticks.  */
		tx_thread_sleep(10);

		/* Set event flag 0 to wakeup thread 5.  */
		status = tx_event_flags_set(&event_flags_0, 0x1, TX_OR);

		/* Check status.  */
		if (status != TX_SUCCESS)
			break;
	}
}

void thread_1_entry(ULONG thread_input)
{

	UINT status;

	/* This thread simply sends messages to a queue shared by thread 2.  */
	while (1)
	{

		/* Increment the thread counter.  */
		thread_1_counter++;

		/* Send message to queue 0.  */
		status = tx_queue_send(&queue_0, &thread_1_messages_sent, TX_WAIT_FOREVER);

		/* Check completion status.  */
		if (status != TX_SUCCESS)
			break;

		/* Increment the message sent.  */
		thread_1_messages_sent++;
	}
}

void thread_2_entry(ULONG thread_input)
{

	ULONG received_message;
	UINT status;

	/* This thread retrieves messages placed on the queue by thread 1.  */
	while (1)
	{

		/* Increment the thread counter.  */
		thread_2_counter++;

		/* Retrieve a message from the queue.  */
		status = tx_queue_receive(&queue_0, &received_message, TX_WAIT_FOREVER);

		/* Check completion status and make sure the message is what we
		   expected.  */
		if ((status != TX_SUCCESS) || (received_message != thread_2_messages_received))
			break;

		/* Otherwise, all is okay.  Increment the received message count.  */
		thread_2_messages_received++;
	}
}

void thread_3_and_4_entry(ULONG thread_input)
{

	UINT status;

	/* This function is executed from thread 3 and thread 4.  As the loop
	   below shows, these function compete for ownership of semaphore_0.  */
	while (1)
	{

		/* Increment the thread counter.  */
		if (thread_input == 3)
			thread_3_counter++;
		else
			thread_4_counter++;

		/* Get the semaphore with suspension.  */
		status = tx_semaphore_get(&semaphore_0, TX_WAIT_FOREVER);

		/* Check status.  */
		if (status != TX_SUCCESS)
			break;

		/* Sleep for 2 ticks to hold the semaphore.  */
		tx_thread_sleep(2);

		/* Release the semaphore.  */
		status = tx_semaphore_put(&semaphore_0);

		/* Check status.  */
		if (status != TX_SUCCESS)
			break;
	}
}

void thread_5_entry(ULONG thread_input)
{

	UINT status;
	ULONG actual_flags;

	/* This thread simply waits for an event in a forever loop.  */
	while (1)
	{

		/* Increment the thread counter.  */
		thread_5_counter++;

		/* Wait for event flag 0.  */
		status = tx_event_flags_get(&event_flags_0, 0x1, TX_OR_CLEAR,
					    &actual_flags, TX_WAIT_FOREVER);

		/* Check status.  */
		if ((status != TX_SUCCESS) || (actual_flags != 0x1))
			break;
	}
}

void thread_6_and_7_entry(ULONG thread_input)
{

	UINT status;

	/* This function is executed from thread 6 and thread 7.  As the loop
	   below shows, these function compete for ownership of mutex_0.  */
	while (1)
	{

		/* Increment the thread counter.  */
		if (thread_input == 6)
			thread_6_counter++;
		else
			thread_7_counter++;

		/* Get the mutex with suspension.  */
		status = tx_mutex_get(&mutex_0, TX_WAIT_FOREVER);

		/* Check status.  */
		if (status != TX_SUCCESS)
			break;

		/* Get the mutex again with suspension.  This shows
		   that an owning thread may retrieve the mutex it
		   owns multiple times.  */
		status = tx_mutex_get(&mutex_0, TX_WAIT_FOREVER);

		/* Check status.  */
		if (status != TX_SUCCESS)
			break;

		/* Sleep for 2 ticks to hold the mutex.  */
		tx_thread_sleep(2);

		/* Release the mutex.  */
		status = tx_mutex_put(&mutex_0);

		/* Check status.  */
		if (status != TX_SUCCESS)
			break;

		/* Release the mutex again.  This will actually
		   release ownership since it was obtained twice.  */
		status = tx_mutex_put(&mutex_0);

		/* Check status.  */
		if (status != TX_SUCCESS)
			break;
	}
}

void SysTick_Handler_h4_IRQHandler()
{
	static volatile uint8_t value = 0u;

	MSS_TIM1_clear_irq(TIMER_LO);

	if (0u == value)
	{
		value = 0x01u;
	}
	else
	{
		value = 0x00u;
	}

	MSS_GPIO_set_output(GPIO2_LO, MSS_GPIO_17, value);
}
