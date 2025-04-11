using System;

namespace WinfordEthIO
{
	// Class to contain event information when we fire a hardware event
	public class Eth32EventArgs : EventArgs
	{
		public readonly eth32_event event_info;

		public Eth32EventArgs(eth32_event ev)
		{
			event_info=ev;

		}
	}


	// Internal class that implements a thread function that will be launched 
	// whenever a nonzero event queue is configured on an Eth32 class.
	internal class event_thread
	{
		WeakReference m_parent;

		public event_thread(Eth32 parent)
		{
			m_parent = new WeakReference(parent, false);
		}

		internal void event_thread_func()
		{
			// This function will be executed in a separate thread and always be trying to 
			// retrieve an event from the event queue.
			int result;
			IntPtr handle;
			eth32_event ev;
			Eth32 parent;
			Eth32EventArgs args;
			Delegate[] delegates;


			try
			{
				parent=m_parent.Target as Eth32;
				if(parent==null)
					return;
				handle=parent.get_handle();


				while(true)
				{
					// We want to avoid having a "strong" reference to our parent Eth32 class
					// so any time we're going to be waiting here, set our parent reference to 
					// null to eliminate the reference
					parent=null;
					result=import.eth32_dequeue_event(handle, out ev, -1);

					if(result!=0)
					{
						// If there was an error, all we can do is quit.  This is most likely
						// due to the event queue being disabled, so it is desirable behavior.
						return;
					}
					args = new Eth32EventArgs(ev);

					parent=m_parent.Target as Eth32;
					if(parent==null)
						return;

					// Now, we're about ready to call the callbacks.  We have to assume that 
					// the callbacks may perform operations on this device.  The only potential
					// problem is that if a Disconnect() call is in progress, it will have the 
					// Writer lock, the callback tries to perform an operation and it will block.
					// Normally, the operation would just be delayed, but in this case, the Disconnect
					// call has to wait for this thread to terminate, so that causes a deadlock.
					// So, we'll get the reader lock ahead of time here and if we fail to do so,
					// we'll inspect the closing flag to see if we should quit.
					if(parent.get_reader_lock(0)==false)
					{
						// Wait until we can get the lock or until the closing flag is set
						do
						{
							if(parent.closing)
								return;
							// Use a nonzero timeout so we don't waste too much CPU time waiting.
						} while(parent.get_reader_lock(50)==false);
					}
					// If we are here in the code, we have the reader lock.
					delegates=parent.get_delegates();
					foreach(Delegate del in delegates)
					{
						EventHandler sink=(EventHandler)del;
						try
						{
							sink(parent, args);
						}
						catch{}
					}
					parent.release_reader_lock();
				}
			} 
			catch
			{
				return;
			}

		}

	}
}
