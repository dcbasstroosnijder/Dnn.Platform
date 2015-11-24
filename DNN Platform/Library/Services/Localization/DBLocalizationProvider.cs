using DotNetNuke.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Services.Localization
{
    public class DBLocalizationProvider : ComponentBase<ILocalizationProvider, DBLocalizationProvider>, ILocalizationProvider
    {
        public string GetString(string key, string resourceFileRoot)
        {
            return GetString(key, resourceFileRoot, null, PortalController.Instance.GetCurrentPortalSettings(), false);
        }

        public string GetString(string key, string resourceFileRoot, string language)
        {
            return GetString(key, resourceFileRoot, language, PortalController.Instance.GetCurrentPortalSettings(), false);
        }

        public string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings)
        {
            return GetString(key, resourceFileRoot, language, portalSettings, false);
        }

        public string GetString(string key, string resourceFileRoot, string language, PortalSettings portalSettings, bool disableShowMissingKeys)
        {
            return "fromDB";
        }

        public bool SaveString(string key, string value, string resourceFileRoot, string language, PortalSettings portalSettings, CustomizedLocale resourceType, bool addFile, bool addKey)
        {
            return true;
        }
    }
}
