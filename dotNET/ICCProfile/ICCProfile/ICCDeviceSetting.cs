﻿/*
  Copyright 2006-2011 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Alexandr Vassilyev (alexandr_vslv@mail.ru)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using PdfClown.Bytes;

namespace ICCProfile
{
    public struct ICCDeviceSetting
    {
        public ICCDeviceSettingsIds SettingId;
        public uint Size;
        public uint Count;
        public ICCDeviceSettingValue[] Values;
        internal void Load(ByteStream buffer)
        {
            SettingId = (ICCDeviceSettingsIds)buffer.ReadUInt32();
            Size = buffer.ReadUInt32();
            Count = buffer.ReadUInt32();
            Values = new ICCDeviceSettingValue[Count];
            for (int i = 0; i < Count; i++)
            {
                var settingValue = (ICCDeviceSettingValue)null;
                if (SettingId == ICCDeviceSettingsIds.Resolution)
                    settingValue = new ICCDeviceSettingResolutiuon();
                if (SettingId == ICCDeviceSettingsIds.MediaType)
                    settingValue = new ICCDeviceSettingMediaType();
                if (SettingId == ICCDeviceSettingsIds.Halftone)
                    settingValue = new ICCDeviceSettingHalftone();
                settingValue.Load(buffer);
                Values[i] = settingValue;
            }
        }
    }
}