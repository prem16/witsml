﻿//----------------------------------------------------------------------- 
// PDS WITSMLstudio Store, 2018.3
//
// Copyright 2018 PDS Americas LLC
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.pds.group/WITSMLstudio/OpenSource/ProductLicenseAgreement
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using Energistics.DataAccess.WITSML200.ComponentSchemas;
using Energistics.DataAccess.WITSML200.ReferenceData;

namespace PDS.WITSMLstudio.Store.Data.WellboreGeometries
{
    /// <summary>
    /// WellboreGeometry200TestBase
    /// </summary>
    public partial class WellboreGeometry200TestBase
    {
        protected override void OnTestSetUp()
        {
            base.OnTestSetUp();

            WellboreGeometry.GrowingStatus = ChannelStatus.active;
            WellboreGeometry.Wellbore = new DataObjectReference
            {
                ContentType = EtpContentTypes.Witsml200.For(ObjectTypes.Wellbore),
                Title = "Wellbore",
                Uuid = DevKit.Uid()
            };
        }
    }
}
