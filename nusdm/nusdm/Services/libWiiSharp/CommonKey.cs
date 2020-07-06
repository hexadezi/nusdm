/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 *
 * libWiiSharp is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * libWiiSharp is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using nusdm;

namespace libWiiSharp
{
    public class CommonKey
    {
        private static string standardKey = SettingsProvider.Settings.CommonKey;
        private static string koreanKey = "";
        private static string vwiiKey = "";

        public static byte[] GetStandardKey()
        {
            return Shared.HexStringToByteArray(standardKey);
        }

        public static byte[] GetKoreanKey()
        {
            return Shared.HexStringToByteArray(koreanKey);
        }

        public static byte[] GetvWiiKey()
        {
            return Shared.HexStringToByteArray(vwiiKey);
        }
    }
}
