using System;

namespace WinfordEthIO
{
	public class LedIndexer
	{
		private Eth32 m_parent;

		public LedIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public bool this [int lednum]
		{
			get
			{
				return(m_parent.get_led(lednum));
			}
			set
			{
				m_parent.set_led(lednum, value);
			}
		}
	}

	public class AnalogAssignmentIndexer
	{
		private Eth32 m_parent;

		public AnalogAssignmentIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public Eth32AnalogChannel  this [int channel]
		{
			get
			{
				return(m_parent.get_analog_assignment(channel));
			}
			set
			{
				m_parent.set_analog_assignment(channel, value);
			}
		}
	}

	public class CounterStateIndexer
	{
		private Eth32 m_parent;

		public CounterStateIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public Eth32CounterState this [int counter]
		{
			get
			{
				return(m_parent.get_counter_state(counter));
			}
			set
			{
				m_parent.set_counter_state(counter, value);
			}
		}
	}

	public class CounterValueIndexer
	{
		private Eth32 m_parent;

		public CounterValueIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public int this [int counter]
		{
			get
			{
				return(m_parent.get_counter_value(counter));
			}
			set
			{
				m_parent.set_counter_value(counter, value);
			}
		}
	}

	public class CounterRolloverIndexer
	{
		private Eth32 m_parent;

		public CounterRolloverIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public int this [int counter]
		{
			get
			{
				return(m_parent.get_counter_rollover(counter));
			}
			set
			{
				m_parent.set_counter_rollover(counter, value);
			}
		}
	}

	public class CounterThresholdIndexer
	{
		private Eth32 m_parent;

		public CounterThresholdIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public int this [int counter]
		{
			get
			{
				return(m_parent.get_counter_threshold(counter));
			}
			set
			{
				m_parent.set_counter_threshold(counter, value);
			}
		}
	}

	public class PwmChannelIndexer
	{
		private Eth32 m_parent;

		public PwmChannelIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public Eth32PwmChannel this [int channel]
		{
			get
			{
				return(m_parent.get_pwm_channel(channel));
			}
			set
			{
				m_parent.set_pwm_channel(channel, value);
			}
		}
	}


	public class PwmDutyPeriodIndexer
	{
		private Eth32 m_parent;

		public PwmDutyPeriodIndexer(Eth32 parent)
		{
			m_parent=parent;
		}

		public int this [int channel]
		{
			get
			{
				return(m_parent.get_pwm_duty_period(channel));
			}
			set
			{
				m_parent.set_pwm_duty_period(channel, value);
			}
		}
	}

    // Indexers for Eth32Config
    public class ConfigResultIndexer
    {
        private Eth32Config m_parent;

        public ConfigResultIndexer(Eth32Config parent)
        {
            m_parent = parent;
        }

        public eth32cfg_data this[int index]
        {
            get
            {
                return (m_parent.get_result(index));
            }
        }
    }

    public class ConfigPluginInterfaceIndexer
    {
        private Eth32ConfigPlugin m_parent;

        public ConfigPluginInterfaceIndexer(Eth32ConfigPlugin parent)
        {
            m_parent = parent;
        }

        public Eth32ConfigPluginInterface this[int index]
        {
            get
            {
                return (m_parent.get_interface(index));
            }
        }

    }
}