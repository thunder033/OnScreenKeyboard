using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ThunderForge.Utilities
{
	/// <summary>
	/// Stores data for an individual OnScreenKeyboard touch key
	/// </summary>
	class TouchKey
	{
		//functional properties of key
		internal int keyIndex;
		internal Keys key;
		internal Keys altKey;

		//display text properties
		internal string displayLower;
		internal string displayUpper;
		internal string displayAlt;

		//controls dimensions and basic properties
		internal float width = 1;
		internal float offset = 0;
		internal float fontScale = 1;
		internal bool repeat = false;

		//fields storing data about the state of the key
		internal float highlightRemaining = 0;
		internal bool pressed = false;
		internal bool altKeyFired = false;
		internal float pressedElapsed;
		internal Color activeColor = Color.Black;

		internal Rectangle boundingBox;
		internal Rectangle drawBox;

		/// <summary>
		/// Stores data for an individual key on the touch keyboard
		/// </summary>
		/// <param name="a_keyIndex">The numerical internal ID of the key</param>
		/// <param name="a_displayLower">The text to display in default mode</param>
		/// <param name="a_displayUpper">The text to display in Shift/CapsLock mode</param>
		/// <param name="a_displayAlt">The text representing the alternate key action, if present</param>
		/// <param name="a_key">The key fired upon activation</param>
		/// <param name="a_altKey">Optional alternate key action</param>
		public TouchKey(int a_keyIndex, string a_displayLower, string a_displayUpper, string a_displayAlt, Keys a_key, Keys a_altKey = Keys.None)
		{
			//set properties
			keyIndex = a_keyIndex;

			displayLower = a_displayLower;
			displayUpper = a_displayUpper;
			displayAlt = a_displayAlt;

			key = a_key;
			altKey = a_altKey;
		}
	}
}