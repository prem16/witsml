//----------------------------------------------------------------------- 
// PDS WITSMLstudio Store, 2017.2
//
// Copyright 2017 PDS Americas LLC
// 
// Licensed under the PDS Open Source WITSML Product License Agreement (the
// "License"); you may not use this file except in compliance with the License.
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

// ----------------------------------------------------------------------
// <auto-generated>
//     Changes to this file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
// ----------------------------------------------------------------------
using System.ComponentModel.Composition;
using Energistics.DataAccess.WITSML200;
using PDS.WITSMLstudio.Framework;

namespace PDS.WITSMLstudio.Store.Data.WellboreGeometries
{
    /// <summary>
    /// Provides validation for <see cref="WellboreGeometry" /> data objects.
    /// </summary>
    /// <seealso cref="PDS.WITSMLstudio.Store.Data.DataObjectValidator{WellboreGeometry}" />
    [Export(typeof(IDataObjectValidator<WellboreGeometry>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class WellboreGeometry200Validator : DataObjectValidator<WellboreGeometry>
    {
        private readonly IWitsmlDataAdapter<WellboreGeometry> _wellboreGeometryDataAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="WellboreGeometry200Validator" /> class.
        /// </summary>
        /// <param name="container">The composition container.</param>
        /// <param name="wellboreGeometryDataAdapter">The wellboreGeometry data adapter.</param>
        [ImportingConstructor]
        public WellboreGeometry200Validator(
            IContainer container,
            IWitsmlDataAdapter<WellboreGeometry> wellboreGeometryDataAdapter)
            : base(container)
        {
            _wellboreGeometryDataAdapter = wellboreGeometryDataAdapter;
        }
    }
}