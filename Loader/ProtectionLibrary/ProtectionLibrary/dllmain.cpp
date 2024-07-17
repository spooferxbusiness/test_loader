#include <Windows.h>

#include "lazy_importer.hpp"
#include "xor_string.hpp"
#include "debugger.hpp"
#include "anti_dump.hpp"
#include "bad_process.hpp"
#include "anti_vm.hpp"
#include "ida.hpp"
#include <thread>

std::uint8_t kill_olly_dbg()
{
	__try {
		(OutputDebugString)(TEXT("%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s%s"));
	}
	__except (EXCEPTION_EXECUTE_HANDLER) { ; }

	return 0;
}

	client::debugger* debugger = new client::debugger();
	client::anti_dump* anti_dump = new client::anti_dump();
	client::anti_vm* anti_vm = new client::anti_vm();
	client::bad_processes* bad_processes = new client::bad_processes();
	client::ida* ida = new client::ida();

	void call()
	{
		while (true)
		{
			kill_olly_dbg();

			if (debugger->is_present() || debugger->remote_is_present() || debugger->thread_context() /*|| debugger->hide_thread()*/ || debugger->debug_string())
			{
				std::thread t([](){
					LI_FN(Sleep).safe()(1900);
					LI_FN(exit).safe()(0);
					});
				LI_FN(MessageBoxA).safe()(NULL, _xor_("Debugger / tampering detected.").c_str(), _xor_("Protection Library").c_str(), NULL);
				LI_FN(exit).safe()(0);
			}

			if (anti_vm->qemu() || anti_vm->vmware() || anti_vm->wine() || anti_vm->xen() || anti_vm->vbox() || anti_vm->vbox_registry())
			{
				std::thread t([](){
					LI_FN(Sleep).safe()(1900);
					LI_FN(exit).safe()(0);
					});
				LI_FN(MessageBoxA).safe()(NULL, _xor_("VM detected.").c_str(), _xor_("Protection Library").c_str(), NULL);
				LI_FN(exit).safe()(0);
			}

			if (bad_processes->check())
			{
				std::thread t([](){
					LI_FN(Sleep).safe()(1900);
					LI_FN(exit).safe()(0);
					});
				LI_FN(MessageBoxA).safe()(NULL, _xor_("Bad Process detected.").c_str(), _xor_("Protection Library").c_str(), NULL);
				LI_FN(exit).safe()(0);
			}

			if (ida->check_history())
			{
				std::thread t([](){
					LI_FN(Sleep).safe()(1900);
					LI_FN(exit).safe()(0);
					});
				LI_FN(MessageBoxA).safe()(NULL, _xor_("IDA Bad History.").c_str(), _xor_("Protection Library").c_str(), NULL);
				LI_FN(exit).safe()(0);
			}
		}

	}


	extern "C" __declspec(dllexport) void init()
	{
		anti_dump->null_size();
		std::thread(&call).detach();

	}
