using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class TextToSpeechService : ITextToSpeechService
{
	public struct RECT
	{
		public int X;

		public int Y;

		public int Right;

		public int Bottom;

		public int Width => Right - X;

		public int Height => Bottom - Y;
	}

	private const int MOUSEEVENTF_LEFTDOWN = 2;

	private const int MOUSEEVENTF_LEFTUP = 4;

	private const int MOUSEEVENTF_RIGHTDOWN = 8;

	private const int MOUSEEVENTF_RIGHTUP = 16;

	private SpeechSynthesizer m_reader;

	private XmlNode[] m_speechControl;

	private XmlNode m_currentSpeech;

	private int m_speechRate = 0;

	private int m_speechVolume = 80;

	private int m_working = 0;

	private Dictionary<string, Delegate> m_keyHandlers = new Dictionary<string, Delegate>();

	private Dictionary<string, string> m_macrosFound = new Dictionary<string, string>();

	private Queue<string> m_textQueue;

	private string m_keySequence;

	private int m_keySequenceDelay;

	public static TextToSpeechService Instance => Singleton<TextToSpeechService>.Instance;

	public bool TTSEnabled => true;

	public void Reset()
	{
		LogHelper.Instance.Log("TTS((> Reset Text-To-Speech service.");
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		if (service != null)
		{
			service.RemoveTimer("TTSTimer");
			service.RemoveTimer("TTSInputTimer");
		}
		SilenceTTS();
		m_keyHandlers.Clear();
		m_speechRate = 0;
		m_speechVolume = 80;
	}

	public void RunSpeechWorkflow(string resourceName)
	{
		m_keyHandlers.Clear();
		if (m_speechControl != null)
		{
			LogHelper.Instance.Log("TTS((> stop existing speech workflow.");
			ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
			if (service != null)
			{
				service.RemoveTimer("TTSTimer");
				service.RemoveTimer("TTSInputTimer");
			}
			SilenceTTS();
			m_keyHandlers.Clear();
		}
		LogHelper.Instance.Log("TTS((> Run speech workflow for " + resourceName);
		m_speechControl = LoadControlMarkup(resourceName);
		if (m_speechControl != null)
		{
			XmlNode xmlNode = m_speechControl[0];
			if (xmlNode != null)
			{
				RunSpeechPart(xmlNode);
			}
		}
	}

	private TextToSpeechService()
	{
		LogHelper.Instance.Log("TTS((> Service Initialized.");
	}

	private void ProcessSpeechPart(XmlNode speechPart)
	{
		XmlNodeList xmlNodeList = speechPart.SelectNodes("text");
		m_textQueue = new Queue<string>();
		if (xmlNodeList != null)
		{
			foreach (XmlNode item in xmlNodeList)
			{
				string text = "";
				string text2 = XmlNodeExtensions.GetAttributeValue<string>(item, "textAvailable");
				string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(item, "ifActorVisible");
				if (text2 != null)
				{
					foreach (KeyValuePair<string, string> item2 in m_macrosFound)
					{
						text2 = text2.Replace("{{" + item2.Key + "}}", item2.Value);
					}
					if (text2.Length > 0)
					{
						text += item.InnerText;
					}
				}
				else if (attributeValue != null)
				{
					IViewService service = ServiceLocator.Instance.GetService<IViewService>();
					IViewFrame viewFrame = service.PeekViewFrame();
					IActor actor = viewFrame.Scene.GetActor(attributeValue);
					if (actor != null && actor.Visible && actor.Enabled)
					{
						text += item.InnerText;
					}
				}
				else
				{
					text += item.InnerText;
				}
				foreach (KeyValuePair<string, string> item3 in m_macrosFound)
				{
					text = text.Replace("{{" + item3.Key + "}}", item3.Value);
				}
				text = text.Replace("\r\n", " ");
				text = text.Replace("\t", "");
				if (text.Length > 0)
				{
					m_textQueue.Enqueue(text);
				}
			}
		}
		if (m_textQueue.Count > 0)
		{
			SayTTS(m_textQueue.Dequeue());
		}
		XmlNode xmlNode2 = speechPart.SelectSingleNode("regex");
		if (xmlNode2 == null)
		{
			return;
		}
		XmlNode xmlNode3 = xmlNode2.SelectSingleNode("value");
		XmlNode xmlNode4 = xmlNode2.SelectSingleNode("expression");
		XmlNode xmlNode5 = xmlNode2.SelectSingleNode("success");
		XmlNode xmlNode6 = xmlNode2.SelectSingleNode("failure");
		if (xmlNode3 == null || xmlNode4 == null || xmlNode5 == null || xmlNode6 == null)
		{
			return;
		}
		string text3 = xmlNode3.InnerText;
		foreach (KeyValuePair<string, string> item4 in m_macrosFound)
		{
			text3 = text3.Replace("{{" + item4.Key + "}}", item4.Value);
		}
		string innerText = xmlNode4.InnerText;
		LogHelper.Instance.Log("TTS((> regex:" + innerText + "  value: " + text3);
		bool flag = new Regex(innerText).IsMatch(text3);
		LogHelper.Instance.Log("TTS((> regex result:" + flag);
		string text4 = (flag ? xmlNode5.InnerText : xmlNode6.InnerText);
		LogHelper.Instance.Log("TTS((> Skipping to Sequence:" + text4);
		int result = 0;
		if (int.TryParse(text4, out result))
		{
			RunSpeechPart(m_speechControl[result]);
		}
	}

	private void RunSpeechPart(XmlNode speechPart)
	{
		int num = -1;
		if (m_currentSpeech != null)
		{
			num = XmlNodeExtensions.GetAttributeValue<int>(m_currentSpeech, "sequence");
		}
		int attributeValue = XmlNodeExtensions.GetAttributeValue<int>(speechPart, "sequence");
		m_currentSpeech = speechPart;
		if (num == attributeValue)
		{
			LogHelper.Instance.Log("TTS((> Repeat speech part #" + attributeValue);
		}
		else
		{
			LogHelper.Instance.Log("TTS((> Run speech part #" + attributeValue);
		}
		m_keyHandlers.Clear();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		m_macrosFound = new Dictionary<string, string>();
		XmlNode xmlNode = speechPart.SelectSingleNode("macrosNeeded");
		if (xmlNode != null)
		{
			XmlNodeList xmlNodeList = xmlNode.SelectNodes("value");
			if (xmlNodeList != null)
			{
				foreach (XmlNode item in xmlNodeList)
				{
					string text = service[item.InnerText];
					if (text != null)
					{
						m_macrosFound.Add(item.InnerText, service[item.InnerText]);
						continue;
					}
					string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(item, "default");
					m_macrosFound.Add(item.InnerText, attributeValue2);
					LogHelper.Instance.Log("TTS((> Required macro {{" + item.InnerText + "}} not found; using default value");
				}
				if (m_macrosFound.Count != xmlNodeList.Count)
				{
					LogHelper.Instance.Log("TTS((> Required text macro lookup mismatch: requires " + xmlNodeList.Count + ", found only " + m_macrosFound.Count);
				}
			}
		}
		ResetTTSEventHandlers(speechPart);
		if (m_currentSpeech.Attributes["keySequenceDelay"] != null)
		{
			m_keySequenceDelay = XmlNodeExtensions.GetAttributeValue<int>(m_currentSpeech, "keySequenceDelay");
		}
		else
		{
			m_keySequenceDelay = 500;
		}
		ITimerService service2 = ServiceLocator.Instance.GetService<ITimerService>();
		if (service2 == null)
		{
			LogHelper.Instance.Log("TTS((> no delay TimerService NULL - passing directly to SayTTs");
			ProcessSpeechPart(speechPart);
			return;
		}
		service2.RemoveTimer("TTSTimer");
		bool attributeValue3 = XmlNodeExtensions.GetAttributeValue(speechPart, "loop", defaultValue: false);
		int attributeValue4 = XmlNodeExtensions.GetAttributeValue(speechPart, "startPause", 0);
		TimerCallback callback = delegate
		{
			ProcessSpeechPart(speechPart);
		};
		if (attributeValue4 == 0)
		{
			LogHelper.Instance.Log("TTS((> no delay startPause = 0 - passing directly to SayTTs");
			ProcessSpeechPart(speechPart);
		}
		else
		{
			LogHelper.Instance.Log("TTS((> dueTime = " + attributeValue4);
			ITimer timer = service2.CreateTimer("TTSTimer", attributeValue4, null, callback);
			timer.Start();
		}
	}

	public void TTSRepeatSequence()
	{
		if (m_currentSpeech != null && m_speechControl != null)
		{
			int attributeValue = XmlNodeExtensions.GetAttributeValue<int>(m_currentSpeech, "sequence");
			RunSpeechPart(m_speechControl[attributeValue]);
		}
	}

	private void ResetTTSEventHandlers(XmlNode speechPart)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		m_keySequence = "";
		service.RemoveKeyPressHandler("TTS-KeyHandler");
		XmlNode xmlNode = speechPart.SelectSingleNode("events");
		if (xmlNode == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNode)
		{
			switch (item.Name)
			{
			case "mapKeyPress":
			{
				string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(item, "keyCode");
				XmlNode xmlNode4 = item.SelectSingleNode("function");
				XmlNode textNode = item.SelectSingleNode("text");
				XmlNode skipToNode = item.SelectSingleNode("value");
				if (xmlNode4 != null)
				{
					switch (xmlNode4.InnerText)
					{
					case "TTSVolumeUp":
					{
						Action value9 = delegate
						{
							TTSVolumeUp();
							string speech = ((textNode != null) ? textNode.InnerText : "Volume Up");
							SayTTS(speech);
						};
						m_keyHandlers.Add(attributeValue2, value9);
						break;
					}
					case "TTSVolumeDown":
					{
						Action value8 = delegate
						{
							TTSVolumeDown();
							string speech = ((textNode != null) ? textNode.InnerText : "Volume Down");
							SayTTS(speech);
						};
						m_keyHandlers.Add(attributeValue2, value8);
						break;
					}
					case "TTSFaster":
					{
						Action value7 = delegate
						{
							TTSFaster();
							SayTTS("Faster");
						};
						m_keyHandlers.Add(attributeValue2, value7);
						break;
					}
					case "TTSSlower":
					{
						Action value6 = delegate
						{
							TTSSlower();
							SayTTS("Slower");
						};
						m_keyHandlers.Add(attributeValue2, value6);
						break;
					}
					case "TTSRepeatSpeechPart":
					{
						Action value5 = delegate
						{
							int attributeValue3 = XmlNodeExtensions.GetAttributeValue<int>(m_currentSpeech, "sequence");
							RunSpeechPart(m_speechControl[attributeValue3]);
						};
						m_keyHandlers.Add(attributeValue2, value5);
						break;
					}
					case "TTSSkipToSpeechPart":
					{
						Action value4 = delegate
						{
							if (skipToNode != null)
							{
								int result = 0;
								if (int.TryParse(skipToNode.InnerText, out result))
								{
									RunSpeechPart(m_speechControl[result]);
								}
							}
						};
						m_keyHandlers.Add(attributeValue2, value4);
						break;
					}
					case "TTSNextSpeechPart":
					{
						Action value3 = delegate
						{
							int attributeValue3 = XmlNodeExtensions.GetAttributeValue<int>(m_currentSpeech, "sequence");
							if (attributeValue3 + 1 <= m_speechControl.GetUpperBound(0))
							{
								ServiceLocator.Instance.GetService<ITimerService>()?.RemoveTimer("TTSTimer");
								RunSpeechPart(m_speechControl[attributeValue3 + 1]);
							}
						};
						m_keyHandlers.Add(attributeValue2, value3);
						break;
					}
					case "TTSNextAudioSnippet":
					{
						Action value2 = delegate
						{
							if (m_textQueue.Count > 0)
							{
								SayTTS(m_textQueue.Dequeue());
							}
							else
							{
								ReaderNothingLeftToSay();
							}
						};
						m_keyHandlers.Add(attributeValue2, value2);
						break;
					}
					}
				}
				else
				{
					if (textNode == null)
					{
						break;
					}
					Action value10 = delegate
					{
						string text = textNode.InnerText;
						foreach (KeyValuePair<string, string> item2 in m_macrosFound)
						{
							text = text.Replace("{{" + item2.Key + "}}", item2.Value);
						}
						SayTTS(text);
					};
					m_keyHandlers.Add(attributeValue2, value10);
				}
				break;
			}
			case "mapKeyPressMultiple":
			{
				XmlNode xmlNode5 = item.SelectSingleNode("function");
				XmlNode textNodeMulti = item.SelectSingleNode("text");
				XmlNode xmlNode6 = item.SelectSingleNode("keyCodes");
				if (xmlNode6 == null)
				{
					break;
				}
				string[] array = xmlNode6.InnerText.Split(',');
				if (xmlNode5 != null || textNodeMulti == null)
				{
					break;
				}
				string[] array2 = array;
				foreach (string key in array2)
				{
					Action value11 = delegate
					{
						SayTTS(textNodeMulti.InnerText);
					};
					m_keyHandlers.Add(key, value11);
				}
				break;
			}
			case "mapKeyPressToActorHit":
			{
				string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(item, "keyCode");
				XmlNode xmlNode3 = item.SelectSingleNode("actorName");
				XmlNode funcNodeActorHit = item.SelectSingleNode("function");
				XmlNode responseTextNode = item.SelectSingleNode("text");
				XmlNode skipToNodeActorHit = item.SelectSingleNode("value");
				if (xmlNode3 == null)
				{
					break;
				}
				string actor_name = xmlNode3.InnerText;
				foreach (KeyValuePair<string, string> item3 in m_macrosFound)
				{
					actor_name = actor_name.Replace("{{" + item3.Key + "}}", item3.Value);
				}
				Action value = delegate
				{
					LogHelper.Instance.Log("TTS((> actorNode.InnerText:" + actor_name);
					LogHelper.Instance.Log("TTS((> actor_name:" + actor_name);
					ClickActor(actor_name);
					if (funcNodeActorHit != null)
					{
						if (responseTextNode != null)
						{
							string text = responseTextNode.InnerText;
							LogHelper.Instance.Log("Say Action: " + text);
							foreach (KeyValuePair<string, string> item4 in m_macrosFound)
							{
								text = text.Replace("{{" + item4.Key + "}}", item4.Value);
							}
							LogHelper.Instance.Log("Say Action: " + text);
							SayTTS(text);
						}
						if (funcNodeActorHit.InnerText == "TTSSkipToSpeechPart" && skipToNodeActorHit != null)
						{
							int result = 0;
							if (int.TryParse(skipToNodeActorHit.InnerText, out result))
							{
								RunSpeechPart(m_speechControl[result]);
							}
						}
					}
				};
				IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
				IViewFrame viewFrame = service2.PeekViewFrame();
				IActor actor = viewFrame.Scene.GetActor(actor_name);
				if (actor != null && actor.Visible && actor.Enabled)
				{
					m_keyHandlers.Add(attributeValue, value);
				}
				break;
			}
			}
		}
		KeyPressHandler handler = delegate(string text, int keyCode, string modifier)
		{
			LogHelper.Instance.Log("TTS((> Key Sequence, mapping:" + text);
			if (text == ".")
			{
				text = "ENTER";
			}
			if (text == "A")
			{
				text = "CANCEL";
			}
			if (text == "B")
			{
				text = "CLEAR";
			}
			if (text == "C")
			{
				text = "PLUS";
			}
			if (text == "*")
			{
				text = "MINUS";
			}
			if (text == "#")
			{
				text = "TAB";
			}
			if (text == "\r")
			{
				text = "ENTER";
			}
			if (text == "c")
			{
				text = "CANCEL";
			}
			if (text == "C")
			{
				text = "CANCEL";
			}
			if (text == "x")
			{
				text = "CANCEL";
			}
			if (text == "X")
			{
				text = "CANCEL";
			}
			if (text == "\b")
			{
				text = "CLEAR";
			}
			if (text == "+")
			{
				text = "PLUS";
			}
			if (text == "-")
			{
				text = "MINUS";
			}
			if (text == "\t")
			{
				text = "TAB";
			}
			if (text == "#")
			{
				text = "TAB";
			}
			LogHelper.Instance.Log("TTS((> Key Sequence, adding:" + text);
			m_keySequence += text;
			ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
			if (m_keySequenceDelay > 0 && timerService != null)
			{
				timerService.RemoveTimer("TTSInputTimer");
				ITimer timer = timerService.CreateTimer("TTSInputTimer", m_keySequenceDelay, null, delegate
				{
					timerService.RemoveTimer("TTSInputTimer");
					TTSFinalizeKeyEntry();
				});
				timer.Start();
			}
			else
			{
				TTSFinalizeKeyEntry();
			}
		};
		service.RegisterKeyPressHandler("TTS-KeyHandler", handler);
	}

	private void TTSFinalizeKeyEntry()
	{
		LogHelper.Instance.Log("TTS((> Final Key Sequence: " + m_keySequence);
		if (m_keyHandlers.ContainsKey(m_keySequence))
		{
			Delegate obj = m_keyHandlers[m_keySequence];
			m_keySequence = "";
			if ((object)obj != null)
			{
				obj.DynamicInvoke();
			}
			else
			{
				SayTTS("Sorry, there was a problem.  Please make your selection again.");
			}
		}
		else
		{
			SayTTS("Sorry, there was a problem.  Please make your selection again.");
		}
		m_keySequence = "";
	}

	private XmlNode[] LoadControlMarkup(string resourceName)
	{
		LogHelper.Instance.Log("TTS((> Load control markup for " + resourceName);
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service != null)
		{
			XmlNode xml = service.GetXml(resourceName);
			if (xml == null)
			{
				return null;
			}
			XmlNodeList xmlNodeList = xml.SelectNodes("speechPart");
			if (xmlNodeList == null)
			{
				return null;
			}
			XmlNode[] array = new XmlNode[xmlNodeList.Count];
			foreach (XmlNode item in xmlNodeList)
			{
				int attributeValue = XmlNodeExtensions.GetAttributeValue(item, "sequence", 0);
				array[attributeValue] = item;
			}
			LogHelper.Instance.Log("TTS((> Loaded [" + (array.GetUpperBound(0) + 1) + "] speech parts into memory.");
			return array;
		}
		return null;
	}

	private void TTSVolumeUp()
	{
		m_speechVolume += 10;
		if (m_speechVolume > 100)
		{
			m_speechVolume = 100;
		}
	}

	private void TTSVolumeDown()
	{
		m_speechVolume -= 10;
		if (m_speechVolume < 0)
		{
			m_speechVolume = 0;
		}
	}

	private void TTSFaster()
	{
		m_speechRate++;
		if (m_speechRate > 10)
		{
			m_speechRate = 10;
		}
	}

	private void TTSSlower()
	{
		m_speechRate--;
		if (m_speechRate < -10)
		{
			m_speechRate = -10;
		}
	}

	private void SayTTS(string speech)
	{
		SilenceTTS();
		ThreadPool.QueueUserWorkItem(delegate
		{
			try
			{
				Interlocked.Increment(ref m_working);
				if (!string.IsNullOrEmpty(speech) && HeadphoneConnected())
				{
					m_reader = new SpeechSynthesizer();
					m_reader.Rate = m_speechRate;
					m_reader.Volume = m_speechVolume;
					LogHelper.Instance.Log("TTS((> Say: " + speech);
					m_reader.SpeakAsync(speech);
					m_reader.SpeakCompleted += ReaderSpeakCompleted;
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An exception was raised in TextToSpeechService:SayTTS()", e);
			}
			finally
			{
				Interlocked.Decrement(ref m_working);
			}
		});
	}

	private void SilenceTTS()
	{
		if (m_reader != null)
		{
			m_reader.Dispose();
			m_reader = null;
		}
	}

	private void RunNextSpeechPart()
	{
		int attributeValue = XmlNodeExtensions.GetAttributeValue<int>(m_currentSpeech, "sequence");
		bool attributeValue2 = XmlNodeExtensions.GetAttributeValue<bool>(m_currentSpeech, "loop");
		int num = (attributeValue2 ? attributeValue : (attributeValue + 1));
		if (attributeValue2 || num < m_speechControl.GetLength(0))
		{
			RunSpeechPart(m_speechControl[num]);
		}
		else
		{
			RunSpeechPart(m_speechControl[0]);
		}
	}

	private void ReaderNothingLeftToSay()
	{
		int attributeValue = XmlNodeExtensions.GetAttributeValue<int>(m_currentSpeech, "sequence");
		LogHelper.Instance.Log("TTS((> Reader speak completed speech part #" + attributeValue);
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		if (service == null)
		{
			return;
		}
		service.RemoveTimer("TTSTimer");
		if (m_currentSpeech == null)
		{
			return;
		}
		int attributeValue2 = XmlNodeExtensions.GetAttributeValue(m_currentSpeech, "endPause", 0);
		if (attributeValue2 > 0)
		{
			ITimer timer = service.CreateTimer("TTSTimer", attributeValue2, null, delegate
			{
				RunNextSpeechPart();
			});
			timer.Start();
		}
		else
		{
			RunNextSpeechPart();
		}
	}

	private void ReaderSpeakCompleted(object sender, SpeakCompletedEventArgs e)
	{
		try
		{
			if (m_textQueue.Count > 0)
			{
				SayTTS(m_textQueue.Dequeue());
			}
			else
			{
				ReaderNothingLeftToSay();
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An exception was raised in TextToSpeechService:SayTTS()", e2);
		}
	}

	private void ClickActor(string actorName)
	{
		LogHelper.Instance.Log("TTS((> Click Actor requested: " + actorName);
		try
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			IViewFrame viewFrame = service.PeekViewFrame();
			IActor actor = viewFrame.Scene.GetActor(actorName);
			if (actor == null)
			{
				LogHelper.Instance.Log("TTS((> actor NOT FOUND in scene.");
				return;
			}
			if (!actor.Visible)
			{
				LogHelper.Instance.Log("TTS((> actor is NOT VISIBLE.");
				return;
			}
			if (!actor.Enabled)
			{
				LogHelper.Instance.Log("TTS((> actor is NOT ENABLED.");
				return;
			}
			int num = FindWindow(null, "Redbox Kiosk Engine");
			if (num > 0)
			{
				RECT lprect = default(RECT);
				GetWindowRect(num, out lprect);
				int x = lprect.X + actor.X + (int)Math.Floor((double)(actor.Width / 2));
				int y = lprect.Y + actor.Y + (int)Math.Floor((double)(actor.Height / 2));
				Cursor.Position = new Point(x, y);
				Thread.Sleep(10);
				MouseClick(x, y);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("TTS((> ClickActor: exception - " + ex.Message);
		}
	}

	private bool HeadphoneConnected()
	{
		return true;
	}

	[DllImport("User32.dll")]
	private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

	[DllImport("User32.dll")]
	private static extern IntPtr FindWindow(string strClassName, int nptWindowName);

	[DllImport("User32.dll")]
	private static extern int FindWindow(string ClassName, string WindowName);

	[DllImport("User32.dll")]
	private static extern bool GetWindowRect(int hWnd, out RECT lprect);

	private void MouseClick(int x, int y)
	{
		mouse_event(2, x, y, 0, 0);
		mouse_event(4, x, y, 0, 0);
	}
}
