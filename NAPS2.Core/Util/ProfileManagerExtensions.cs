namespace NAPS2.Util
{
    using System.Linq;

    using NAPS2.Config;
    using NAPS2.Scan;
    using NAPS2.WinForms;

    /// <summary>
    /// Extension methods for the profile manager.
    /// </summary>
    public static class ProfileManagerExtensions
    {
        /// <summary>
        ///     Gets the default scan profile, or if no profiles have yet been created, creates a new one and sets as default.
        /// </summary>
        /// <param name="profileManager">The profile manager.</param>
        /// <param name="formFactory">The form factory.</param>
        /// <param name="appConfigManager">The application configuration manager.</param>
        /// <returns>The scan profile that should be used for a scan.</returns>
        public static ScanProfile DefaultOrNewProfile(
            this IProfileManager profileManager,
            IFormFactory formFactory,
            AppConfigManager appConfigManager)
        {
            return profileManager.Profiles.Any()
                       ? profileManager.DefaultProfile
                       : NewProfile(profileManager, formFactory, appConfigManager);
        }

        /// <summary>
        ///     Gets a new scan profile.
        /// </summary>
        /// <param name="profileManager">The profile manager.</param>
        /// <param name="formFactory">The form factory.</param>
        /// <param name="appConfigManager">The application configuration manager.</param>
        /// <returns>A new scan profile.</returns>
        public static ScanProfile NewProfile(
            this IProfileManager profileManager,
            IFormFactory formFactory,
            AppConfigManager appConfigManager)
        {
            var editSettingsForm = formFactory.Create<FEditProfile>();
            editSettingsForm.ScanProfile = appConfigManager.Config.DefaultProfileSettings
                                           ?? new ScanProfile { Version = ScanProfile.CURRENT_VERSION };
            editSettingsForm.ShowDialog();

            ScanProfile scanProfile = editSettingsForm.Result ? editSettingsForm.ScanProfile : null;

            if (scanProfile != null)
            {
                profileManager.Profiles.Add(editSettingsForm.ScanProfile);
                profileManager.DefaultProfile = editSettingsForm.ScanProfile;
                profileManager.Save();
            }

            return scanProfile;
        }
    }
}