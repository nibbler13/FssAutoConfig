using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FssAutoConfig {
	public class Conversion {
		//converted from javascript function at https://raw.githubusercontent.com/r12a/r12a.github.io/master/apps/conversion/conversionfunctions.js
		//original site: https://r12a.github.io/apps/conversion/
		public static string ConvertCharStr2jEsc(string str) {
			// Converts a string of characters to JavaScript escapes
			// str: sequence of Unicode characters
			// parameters: a semicolon separated string showing ids for checkboxes that are turned on
			int highsurrogate = 0;
			int suppCP;
			string pad;
			string outputString = string.Empty;

			for (int i = 0; i < str.Length; i++) {
				char original = str.ToCharArray()[i];
				int cc = str.ToCharArray()[i];

				if (cc < 0 || cc > 0xFFFF)
					outputString += "!Error in convertCharStr2UTF16: unexpected charCodeAt result, cc=" + cc + "!";

				if (highsurrogate != 0) {
					// this is a supp char, and cc contains the low surrogate
					if (0xDC00 <= cc && cc <= 0xDFFF) {
						suppCP = 0x10000 + ((highsurrogate - 0xD800) << 10) + (cc - 0xDC00);
						suppCP -= 0x10000;
						outputString += "\\u" + Dec2hex4(0xD800 | (suppCP >> 10)) + "\\u" + Dec2hex4(0xDC00 | (suppCP & 0x3FF));
						highsurrogate = 0;
						continue;
					} else {
						outputString += "Error in convertCharStr2UTF16: low surrogate expected, cc=" + cc + "!";
						highsurrogate = 0;
					}
				}

				if (0xD800 <= cc && cc <= 0xDBFF) { 
					// start of supplementary character
					highsurrogate = cc;
				} else { 
					// this is a BMP character
					//outputString += dec2hex(cc) + ' ';
					switch (cc) {
						case 0: outputString += "\\0"; break;
						case 8: outputString += "\\b"; break;
						case 9: outputString += "\t"; break;
						case 10: outputString += "\n"; break;
						case 13: outputString += "\r"; break;
						case 11: outputString += "\\v"; break;
						case 12: outputString += "\\f"; break;
						case 34: outputString += '"'; break;
						case 39: outputString += "'"; break;
						case 92: outputString += "\\\\"; break;
						default:
							if (cc > 0x1f && cc < 0x7F) {
								//outputString += String.fromCharCode(cc);
								outputString += original;
							} else {
								//pad = cc.toString(16).toUpperCase();
								pad = cc.ToString("X").ToUpper();

								while (pad.Length < 4)
									pad = '0' + pad;

								outputString += "\\u" + pad;
	  						}

							break;
					}
				}
			}
			return outputString;
		}
		
		private static string Dec2hex4(int textString) {
			string[] hexequiv = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
			return 
				hexequiv[(textString >> 12) & 0xF] + 
				hexequiv[(textString >> 8) & 0xF] + 
				hexequiv[(textString >> 4) & 0xF] +  
				hexequiv[textString & 0xF];
		}
	}
}
