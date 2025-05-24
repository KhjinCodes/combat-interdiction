using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;

namespace Khjin.CombatInterdiction
{
    public class CombatInterdictionSettings
    {
        private const string configversion = "1.0.6";
        private const string SECTION_NAME = "Combat Interdiction Settings";
        private const string SETTINGS_FILENAME = "combat_interdiction_settings.cfg";
        private MyIni iniUtil = null;

        // Global Settings
        public bool allowSuperCruise;
        public float globalSmallGridMaxSpeed;
        public float globalLargeGridMaxSpeed;
        public float smallGridMaxSpeed;
        public float largeGridMaxSpeed;
        public float minimumGridVolume;

        // Boost and Combat
        public float smallGridBoostSpeedMultiplier;
        public float largeGridBoostSpeedMultiplier;
        public float largeGridBoostTwr;
        public int interdictionDuration;
        public float combatZoneRadius;

        // Large grid atmo/ion based ships
        public float largeGridBaseWeight;
        public float largeGridBaseTwr;
        public float largeGridMinimumTwr;
        public float largeGridMaximumTwr;
        public float largeGridSpeedFactor;
        public float largeGridWeightFactor;
        public float largeGridBaseTurnRate;
        public float largeGridBaseTurnRateSpeed;
        public float largeGridMinimumTurnRate;
        public float largeGridMaximumTurnRate;
        public float largeGridTurnRateWeightFactor;
        public float largeGridTurnRateSpeedFactor;

        // Large grid gas based ships
        public float largeGridJetBaseWeight;
        public float largeGridJetBaseTwr;
        public float largeGridJetMinimumTwr;
        public float largeGridJetMaximumTwr;
        public float largeGridJetSpeedFactor;
        public float largeGridJetWeightFactor;
        public float largeGridJetBaseTurnRate;
        public float largeGridJetBaseTurnRateSpeed;
        public float largeGridJetMinimumTurnRate;
        public float largeGridJetMaximumTurnRate;
        public float largeGridJetTurnRateWeightFactor;
        public float largeGridJetTurnRateSpeedFactor;

        // Small grid atmo/ion based ships
        public float smallGridBaseWeight;
        public float smallGridBaseTwr;
        public float smallGridMinimumTwr;
        public float smallGridMaximumTwr;
        public float smallGridSpeedFactor;
        public float smallGridWeightFactor;
        public float smallGridBaseTurnRate;
        public float smallGridBaseTurnRateSpeed;
        public float smallGridMinimumTurnRate;
        public float smallGridMaximumTurnRate;
        public float smallGridTurnRateWeightFactor;
        public float smallGridTurnRateSpeedFactor;

        // Small grid gas based ships
        public float smallGridJetBaseWeight;
        public float smallGridJetBaseTwr;
        public float smallGridJetMinimumTwr;
        public float smallGridJetMaximumTwr;
        public float smallGridJetSpeedFactor;
        public float smallGridJetWeightFactor;
        public float smallGridJetBaseTurnRate;
        public float smallGridJetBaseTurnRateSpeed;
        public float smallGridJetMinimumTurnRate;
        public float smallGridJetMaximumTurnRate;
        public float smallGridJetTurnRateWeightFactor;
        public float smallGridJetTurnRateSpeedFactor;


        private Dictionary<string, SettingLimits> settingLimits;

        public CombatInterdictionSettings()
        {
            iniUtil = new MyIni();
            settingLimits = new Dictionary<string, SettingLimits>();
        }

        public void LoadData()
        {
            // Global Settings
            settingLimits.Add(nameof(allowSuperCruise), new BoolLimits()
            {
                DefaultValue = true,
            });
            settingLimits.Add(nameof(globalLargeGridMaxSpeed), new FloatLimits() 
            { 
                DefaultValue = 500,
                MinValue = 5,
                MaxValue = 600
            });
            settingLimits.Add(nameof(globalSmallGridMaxSpeed), new FloatLimits()
            {
                DefaultValue = 500,
                MinValue = 5,
                MaxValue = 600
            });
            settingLimits.Add(nameof(largeGridMaxSpeed), new FloatLimits()
            {
                DefaultValue = 150.0f,
                MinValue = 5,
                MaxValue = 600,
            });
            settingLimits.Add(nameof(smallGridMaxSpeed), new FloatLimits()
            {
                DefaultValue = 300.0f,
                MinValue = 5,
                MaxValue = 600,
            });
            settingLimits.Add(nameof(minimumGridVolume), new FloatLimits()
            {
                DefaultValue = 1.5f * 1.5f * 10.0f,
                MinValue = 0.5f * 0.5f * 0.5f,
                MaxValue = 1000.0f * 1000.0f * 10000.0f,
            });

            // Boost and Combat
            settingLimits.Add(nameof(smallGridBoostSpeedMultiplier), new FloatLimits()
            {
                DefaultValue = 1.45f,
                MinValue = 1.0f,
                MaxValue = 20.0f
            });
            settingLimits.Add(nameof(largeGridBoostSpeedMultiplier), new FloatLimits()
            {
                DefaultValue = 6.5f,
                MinValue = 1.0f,
                MaxValue = 20.0f
            });
            settingLimits.Add(nameof(largeGridBoostTwr), new FloatLimits()
            {
                DefaultValue = 15.0f,
                MinValue = 0.001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(interdictionDuration), new IntLimits()
            {
                DefaultValue = 120,
                MinValue = 0,
                MaxValue = 300
            });
            settingLimits.Add(nameof(combatZoneRadius), new FloatLimits()
            {
                DefaultValue = 15000,
                MinValue = 500,
                MaxValue = 1000000,
            });

            // Large grid atmo/ion based ships
            settingLimits.Add(nameof(largeGridBaseWeight), new FloatLimits()
            {
                DefaultValue = 9700000.0f,
                MinValue = 100000.0f,
                MaxValue = 900000000.0f
            });
            settingLimits.Add(nameof(largeGridBaseTwr), new FloatLimits()
            {
                DefaultValue = 0.046f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridMinimumTwr), new FloatLimits()
            {
                DefaultValue = 0.02f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridMaximumTwr), new FloatLimits()
            {
                DefaultValue = 0.08f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridSpeedFactor), new FloatLimits()
            {
                DefaultValue = 1210f,
                MinValue = 0.00001f,
                MaxValue = 100000.0f
            });
            settingLimits.Add(nameof(largeGridWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.769f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridBaseTurnRate), new FloatLimits()
            {
                DefaultValue = 20.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(largeGridBaseTurnRateSpeed), new FloatLimits()
            {
                DefaultValue = 660f,
                MinValue = 1.0f,
                MaxValue = 1000.0f
            });
            settingLimits.Add(nameof(largeGridMinimumTurnRate), new FloatLimits()
            {
                DefaultValue = 5.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(largeGridMaximumTurnRate), new FloatLimits()
            {
                DefaultValue = 25.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(largeGridTurnRateWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.5f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridTurnRateSpeedFactor), new FloatLimits()
            {
                DefaultValue = 0.3f,
                MinValue = 0.0001f,
                MaxValue = 100.0f
            });

            // Large grid gas based ships
            settingLimits.Add(nameof(largeGridJetBaseWeight), new FloatLimits()
            {
                DefaultValue = 9700000.0f,
                MinValue = 100000.0f,
                MaxValue = 900000000.0f
            });
            settingLimits.Add(nameof(largeGridJetBaseTwr), new FloatLimits()
            {
                DefaultValue = 0.046f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridJetMinimumTwr), new FloatLimits()
            {
                DefaultValue = 0.02f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridJetMaximumTwr), new FloatLimits()
            {
                DefaultValue = 0.08f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridJetSpeedFactor), new FloatLimits()
            {
                DefaultValue = 1210f,
                MinValue = 0.00001f,
                MaxValue = 100000.0f
            });
            settingLimits.Add(nameof(largeGridJetWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.769f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridJetBaseTurnRate), new FloatLimits()
            {
                DefaultValue = 20.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(largeGridJetBaseTurnRateSpeed), new FloatLimits()
            {
                DefaultValue = 660f,
                MinValue = 1.0f,
                MaxValue = 1000.0f
            });
            settingLimits.Add(nameof(largeGridJetMinimumTurnRate), new FloatLimits()
            {
                DefaultValue = 5.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(largeGridJetMaximumTurnRate), new FloatLimits()
            {
                DefaultValue = 25.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(largeGridJetTurnRateWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.5f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(largeGridJetTurnRateSpeedFactor), new FloatLimits()
            {
                DefaultValue = 0.3f,
                MinValue = 0.0001f,
                MaxValue = 100.0f
            });

            // Small grid atmo/ion based ships
            settingLimits.Add(nameof(smallGridBaseWeight), new FloatLimits()
            {
                DefaultValue = 15000.0f,
                MinValue = 1000.0f,
                MaxValue = 1000000.0f
            });
            settingLimits.Add(nameof(smallGridBaseTwr), new FloatLimits()
            {
                DefaultValue = 0.27f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(smallGridMinimumTwr), new FloatLimits()
            {
                DefaultValue = 0.19f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(smallGridMaximumTwr), new FloatLimits()
            {
                DefaultValue = 0.30f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(smallGridSpeedFactor), new FloatLimits()
            {
                DefaultValue = 15.15f,
                MinValue = 0.00001f,
                MaxValue = 1000.0f
            });
            settingLimits.Add(nameof(smallGridWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.165f,
                MinValue = 0.00001f,
                MaxValue = 10.0f
            });
            settingLimits.Add(nameof(smallGridBaseTurnRate), new FloatLimits()
            {
                DefaultValue = 20f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(smallGridBaseTurnRateSpeed), new FloatLimits()
            {
                DefaultValue = 560f,
                MinValue = 1.0f,
                MaxValue = 1000.0f
            });
            settingLimits.Add(nameof(smallGridMinimumTurnRate), new FloatLimits()
            {
                DefaultValue = 5.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(smallGridMaximumTurnRate), new FloatLimits()
            {
                DefaultValue = 27.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(smallGridTurnRateWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.5f,
                MinValue = 0.00001f,
                MaxValue = 10.0f
            });
            settingLimits.Add(nameof(smallGridTurnRateSpeedFactor), new FloatLimits()
            {
                DefaultValue = 0.3f,
                MinValue = 0.00001f,
                MaxValue = 10.0f
            });

            // Small grid gas based ships
            settingLimits.Add(nameof(smallGridJetBaseWeight), new FloatLimits()
            {
                DefaultValue = 15000.0f,
                MinValue = 1000.0f,
                MaxValue = 1000000.0f
            });
            settingLimits.Add(nameof(smallGridJetBaseTwr), new FloatLimits()
            {
                DefaultValue = 1.11f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(smallGridJetMinimumTwr), new FloatLimits()
            {
                DefaultValue = 0.40f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(smallGridJetMaximumTwr), new FloatLimits()
            {
                DefaultValue = 1.45f,
                MinValue = 0.00001f,
                MaxValue = 100.0f
            });
            settingLimits.Add(nameof(smallGridJetSpeedFactor), new FloatLimits()
            {
                DefaultValue = 4.41f,
                MinValue = 0.00001f,
                MaxValue = 1000.0f
            });
            settingLimits.Add(nameof(smallGridJetWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.22f,
                MinValue = 0.00001f,
                MaxValue = 10.0f
            });
            settingLimits.Add(nameof(smallGridJetBaseTurnRate), new FloatLimits()
            {
                DefaultValue = 18.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(smallGridJetBaseTurnRateSpeed), new FloatLimits()
            {
                DefaultValue = 680f,
                MinValue = 1.0f,
                MaxValue = 1000.0f
            });
            settingLimits.Add(nameof(smallGridJetMinimumTurnRate), new FloatLimits()
            {
                DefaultValue = 5.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(smallGridJetMaximumTurnRate), new FloatLimits()
            {
                DefaultValue = 25.0f,
                MinValue = 1.0f,
                MaxValue = 90.0f
            });
            settingLimits.Add(nameof(smallGridJetTurnRateWeightFactor), new FloatLimits()
            {
                DefaultValue = 0.5f,
                MinValue = 0.00001f,
                MaxValue = 10.0f
            });
            settingLimits.Add(nameof(smallGridJetTurnRateSpeedFactor), new FloatLimits()
            {
                DefaultValue = 0.3f,
                MinValue = 0.00001f,
                MaxValue = 10.0f
            });

            // Note: This must go AFTER defining the limits as defaults
            // will also come from the limits data.
            LoadSettings();
        }

        public void UnloadData()
        {
            SaveSettings();

            iniUtil.Clear();
            iniUtil = null;

            settingLimits.Clear();
            settingLimits = null;
        }

        public void ResetSettings()
        {
            // Global Settings
            allowSuperCruise = ((BoolLimits)settingLimits[nameof(allowSuperCruise)]).DefaultValue;
            globalLargeGridMaxSpeed = ((FloatLimits)settingLimits[nameof(globalLargeGridMaxSpeed)]).DefaultValue;
            globalSmallGridMaxSpeed = ((FloatLimits)settingLimits[nameof(globalSmallGridMaxSpeed)]).DefaultValue;
            largeGridMaxSpeed = ((FloatLimits)settingLimits[nameof(largeGridMaxSpeed)]).DefaultValue;
            smallGridMaxSpeed = ((FloatLimits)settingLimits[nameof(smallGridMaxSpeed)]).DefaultValue;
            minimumGridVolume = ((FloatLimits)settingLimits[nameof(minimumGridVolume)]).DefaultValue;

            // Boost and Combat
            smallGridBoostSpeedMultiplier = ((FloatLimits)settingLimits[nameof(smallGridBoostSpeedMultiplier)]).DefaultValue;
            largeGridBoostSpeedMultiplier = ((FloatLimits)settingLimits[nameof(largeGridBoostSpeedMultiplier)]).DefaultValue;
            largeGridBoostTwr = ((FloatLimits)settingLimits[nameof(largeGridBoostTwr)]).DefaultValue;
            interdictionDuration = ((IntLimits)settingLimits[nameof(interdictionDuration)]).DefaultValue;
            combatZoneRadius = ((FloatLimits)settingLimits[nameof(combatZoneRadius)]).DefaultValue;

            // Large grid atmo/ion based ships
            largeGridBaseWeight = ((FloatLimits)settingLimits[nameof(largeGridBaseWeight)]).DefaultValue;
            largeGridBaseTwr = ((FloatLimits)settingLimits[nameof(largeGridBaseTwr)]).DefaultValue;
            largeGridMinimumTwr = ((FloatLimits)settingLimits[nameof(largeGridMinimumTwr)]).DefaultValue;
            largeGridMaximumTwr = ((FloatLimits)settingLimits[nameof(largeGridMaximumTwr)]).DefaultValue;
            largeGridSpeedFactor = ((FloatLimits)settingLimits[nameof(largeGridSpeedFactor)]).DefaultValue;
            largeGridWeightFactor = ((FloatLimits)settingLimits[nameof(largeGridWeightFactor)]).DefaultValue;
            largeGridBaseTurnRate = ((FloatLimits)settingLimits[nameof(largeGridBaseTurnRate)]).DefaultValue;
            largeGridMinimumTurnRate = ((FloatLimits)settingLimits[nameof(largeGridMinimumTurnRate)]).DefaultValue;
            largeGridMaximumTurnRate = ((FloatLimits)settingLimits[nameof(largeGridMaximumTurnRate)]).DefaultValue;
            largeGridTurnRateWeightFactor = ((FloatLimits)settingLimits[nameof(largeGridTurnRateWeightFactor)]).DefaultValue;
            largeGridTurnRateSpeedFactor = ((FloatLimits)settingLimits[nameof(largeGridTurnRateSpeedFactor)]).DefaultValue;

            // Large grid gas based ships
            largeGridJetBaseWeight = ((FloatLimits)settingLimits[nameof(largeGridJetBaseWeight)]).DefaultValue;
            largeGridJetBaseTwr = ((FloatLimits)settingLimits[nameof(largeGridJetBaseTwr)]).DefaultValue;
            largeGridJetMinimumTwr = ((FloatLimits)settingLimits[nameof(largeGridJetMinimumTwr)]).DefaultValue;
            largeGridJetMaximumTwr = ((FloatLimits)settingLimits[nameof(largeGridJetMaximumTwr)]).DefaultValue;
            largeGridJetSpeedFactor = ((FloatLimits)settingLimits[nameof(largeGridJetSpeedFactor)]).DefaultValue;
            largeGridJetWeightFactor = ((FloatLimits)settingLimits[nameof(largeGridJetWeightFactor)]).DefaultValue;
            largeGridJetBaseTurnRate = ((FloatLimits)settingLimits[nameof(largeGridJetBaseTurnRate)]).DefaultValue;
            largeGridJetMinimumTurnRate = ((FloatLimits)settingLimits[nameof(largeGridJetMinimumTurnRate)]).DefaultValue;
            largeGridJetMaximumTurnRate = ((FloatLimits)settingLimits[nameof(largeGridJetMaximumTurnRate)]).DefaultValue;
            largeGridJetTurnRateWeightFactor = ((FloatLimits)settingLimits[nameof(largeGridJetTurnRateWeightFactor)]).DefaultValue;
            largeGridJetTurnRateSpeedFactor = ((FloatLimits)settingLimits[nameof(largeGridJetTurnRateSpeedFactor)]).DefaultValue;

            // Small grid atmo/ion based ships
            smallGridBaseWeight = ((FloatLimits)settingLimits[nameof(smallGridBaseWeight)]).DefaultValue;
            smallGridBaseTwr = ((FloatLimits)settingLimits[nameof(smallGridBaseTwr)]).DefaultValue;
            smallGridMinimumTwr = ((FloatLimits)settingLimits[nameof(smallGridMinimumTwr)]).DefaultValue;
            smallGridMaximumTwr = ((FloatLimits)settingLimits[nameof(smallGridMaximumTwr)]).DefaultValue;
            smallGridSpeedFactor = ((FloatLimits)settingLimits[nameof(smallGridSpeedFactor)]).DefaultValue;
            smallGridWeightFactor = ((FloatLimits)settingLimits[nameof(smallGridWeightFactor)]).DefaultValue;
            smallGridBaseTurnRate = ((FloatLimits)settingLimits[nameof(smallGridBaseTurnRate)]).DefaultValue;
            smallGridMinimumTurnRate = ((FloatLimits)settingLimits[nameof(smallGridMinimumTurnRate)]).DefaultValue;
            smallGridMaximumTurnRate = ((FloatLimits)settingLimits[nameof(smallGridMaximumTurnRate)]).DefaultValue;
            smallGridTurnRateWeightFactor = ((FloatLimits)settingLimits[nameof(smallGridTurnRateWeightFactor)]).DefaultValue;
            smallGridTurnRateSpeedFactor = ((FloatLimits)settingLimits[nameof(smallGridTurnRateSpeedFactor)]).DefaultValue;

            // Small grid gas based ships
            smallGridJetBaseWeight = ((FloatLimits)settingLimits[nameof(smallGridJetBaseWeight)]).DefaultValue;
            smallGridJetBaseTwr = ((FloatLimits)settingLimits[nameof(smallGridJetBaseTwr)]).DefaultValue;
            smallGridJetMinimumTwr = ((FloatLimits)settingLimits[nameof(smallGridJetMinimumTwr)]).DefaultValue;
            smallGridJetMaximumTwr = ((FloatLimits)settingLimits[nameof(smallGridJetMaximumTwr)]).DefaultValue;
            smallGridJetSpeedFactor = ((FloatLimits)settingLimits[nameof(smallGridJetSpeedFactor)]).DefaultValue;
            smallGridJetWeightFactor = ((FloatLimits)settingLimits[nameof(smallGridJetWeightFactor)]).DefaultValue;
            smallGridJetBaseTurnRate = ((FloatLimits)settingLimits[nameof(smallGridJetBaseTurnRate)]).DefaultValue;
            smallGridJetMinimumTurnRate = ((FloatLimits)settingLimits[nameof(smallGridJetMinimumTurnRate)]).DefaultValue;
            smallGridJetMaximumTurnRate = ((FloatLimits)settingLimits[nameof(smallGridJetMaximumTurnRate)]).DefaultValue;
            smallGridJetTurnRateWeightFactor = ((FloatLimits)settingLimits[nameof(smallGridJetTurnRateWeightFactor)]).DefaultValue;
            smallGridJetTurnRateSpeedFactor = ((FloatLimits)settingLimits[nameof(smallGridJetTurnRateSpeedFactor)]).DefaultValue;
        }

        private void LoadSettings()
        {
            try
            {
                // Search settings in the world save
                if (MyAPIGateway.Utilities.FileExistsInWorldStorage(SETTINGS_FILENAME, typeof(CombatInterdictionSettings)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(SETTINGS_FILENAME, typeof(CombatInterdictionSettings));
                    string settingsData = reader.ReadToEnd();

                    iniUtil.Clear();
                    if (iniUtil.TryParse(settingsData))
                    {
                        MyIniValue value = iniUtil.Get(SECTION_NAME, nameof(configversion));
                        if (value.IsEmpty || value.ToString() != configversion)
                        {
                            ResetSettings();
                            SaveSettings();
                            return;
                        }
                        ReadSettings();
                    }
                    else
                    {
                        ResetSettings();
                        ShowParseError();
                    }
                }
                else
                {
                    // Not yet existing so we make one
                    ResetSettings();
                    SaveSettings();
                }
            }
            catch (Exception)
            {
                ResetSettings();
                ShowParseError();
            }
        }

        private void SaveSettings()
        {
            if (MyAPIGateway.Utilities.FileExistsInWorldStorage(SETTINGS_FILENAME, typeof(CombatInterdictionSettings)))
            {
                MyAPIGateway.Utilities.DeleteFileInWorldStorage(SETTINGS_FILENAME, typeof(CombatInterdictionSettings));
            }
            var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(SETTINGS_FILENAME, typeof(CombatInterdictionSettings));

            iniUtil.Clear();
            iniUtil.AddSection(SECTION_NAME);

            WriteSettings();

            writer.Write(iniUtil.ToString());
            writer.Close();
        }

        private void ReadSettings()
        {
            // Global Settings
            allowSuperCruise = iniUtil.Get(SECTION_NAME, nameof(allowSuperCruise)).ToBoolean();
            globalLargeGridMaxSpeed = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridBoostSpeedMultiplier)).ToDouble();
            globalSmallGridMaxSpeed = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridBoostSpeedMultiplier)).ToDouble();
            largeGridMaxSpeed = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridMaxSpeed)).ToDouble();
            smallGridMaxSpeed = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridMaxSpeed)).ToDouble();
            minimumGridVolume = (float)iniUtil.Get(SECTION_NAME, nameof(minimumGridVolume)).ToDouble();

            // Boost and Combat
            smallGridBoostSpeedMultiplier = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridBoostSpeedMultiplier)).ToDouble();
            largeGridBoostSpeedMultiplier = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridBoostSpeedMultiplier)).ToDouble();
            largeGridBoostTwr = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridBoostTwr)).ToDouble();
            interdictionDuration = iniUtil.Get(SECTION_NAME, nameof(interdictionDuration)).ToInt32();
            combatZoneRadius = (float)iniUtil.Get(SECTION_NAME, nameof(combatZoneRadius)).ToDouble();

            // Large grid atmo/ion based ships
            largeGridBaseWeight = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridBaseWeight)).ToDouble();
            largeGridBaseTwr = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridBaseTwr)).ToDouble();
            largeGridMinimumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridMinimumTwr)).ToDouble();
            largeGridMaximumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridMaximumTwr)).ToDouble();
            largeGridSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridSpeedFactor)).ToDouble();
            largeGridWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridWeightFactor)).ToDouble();
            largeGridBaseTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridBaseTurnRate)).ToDouble();
            largeGridMinimumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridMinimumTurnRate)).ToDouble();
            largeGridMaximumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridMaximumTurnRate)).ToDouble();
            largeGridTurnRateWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridTurnRateWeightFactor)).ToDouble();
            largeGridTurnRateSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridTurnRateSpeedFactor)).ToDouble();

            // Large grid gas based ships
            largeGridJetBaseWeight = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetBaseWeight)).ToDouble();
            largeGridJetBaseTwr = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetBaseTwr)).ToDouble();
            largeGridJetMinimumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetMinimumTwr)).ToDouble();
            largeGridJetMaximumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetMaximumTwr)).ToDouble();
            largeGridJetSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetSpeedFactor)).ToDouble();
            largeGridJetWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetWeightFactor)).ToDouble();
            largeGridJetBaseTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetBaseTurnRate)).ToDouble();
            largeGridJetMinimumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetMinimumTurnRate)).ToDouble();
            largeGridJetMaximumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetMaximumTurnRate)).ToDouble();
            largeGridJetTurnRateWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetTurnRateWeightFactor)).ToDouble();
            largeGridJetTurnRateSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetTurnRateSpeedFactor)).ToDouble();

            // Small grid atmo/ion based ships
            smallGridBaseWeight = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridBaseWeight)).ToDouble();
            smallGridBaseTwr = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridBaseTwr)).ToDouble();
            smallGridMinimumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridMinimumTwr)).ToDouble();
            smallGridMaximumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridMaximumTwr)).ToDouble();
            smallGridSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridSpeedFactor)).ToDouble();
            smallGridWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridWeightFactor)).ToDouble();
            smallGridBaseTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridBaseTurnRate)).ToDouble();
            smallGridMinimumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridMinimumTurnRate)).ToDouble();
            smallGridMaximumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridMaximumTurnRate)).ToDouble();
            smallGridTurnRateWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridTurnRateWeightFactor)).ToDouble();
            smallGridTurnRateSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridTurnRateSpeedFactor)).ToDouble();

            // Small grid gas based ships
            smallGridJetBaseWeight = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetBaseWeight)).ToDouble();
            smallGridJetBaseTwr = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetBaseTwr)).ToDouble();
            smallGridJetMinimumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetMinimumTwr)).ToDouble();
            smallGridJetMaximumTwr = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetMaximumTwr)).ToDouble();
            smallGridJetSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetSpeedFactor)).ToDouble();
            smallGridJetWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetWeightFactor)).ToDouble();
            smallGridJetBaseTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetBaseTurnRate)).ToDouble();
            smallGridJetMinimumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetMinimumTurnRate)).ToDouble();
            smallGridJetMaximumTurnRate = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetMaximumTurnRate)).ToDouble();
            smallGridJetTurnRateWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetTurnRateWeightFactor)).ToDouble();
            smallGridJetTurnRateSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetTurnRateSpeedFactor)).ToDouble();
        }

        private void WriteSettings()
        {
            // Global Settings
            iniUtil.Set(SECTION_NAME, nameof(configversion), configversion);
            iniUtil.Set(SECTION_NAME, nameof(allowSuperCruise), allowSuperCruise);
            iniUtil.Set(SECTION_NAME, nameof(globalLargeGridMaxSpeed), globalLargeGridMaxSpeed);
            iniUtil.Set(SECTION_NAME, nameof(globalSmallGridMaxSpeed), globalSmallGridMaxSpeed);
            iniUtil.Set(SECTION_NAME, nameof(largeGridMaxSpeed), largeGridMaxSpeed);
            iniUtil.Set(SECTION_NAME, nameof(smallGridMaxSpeed), smallGridMaxSpeed);
            iniUtil.Set(SECTION_NAME, nameof(minimumGridVolume), minimumGridVolume);

            // Boost and Combat
            iniUtil.Set(SECTION_NAME, nameof(smallGridBoostSpeedMultiplier), smallGridBoostSpeedMultiplier);
            iniUtil.Set(SECTION_NAME, nameof(largeGridBoostSpeedMultiplier), largeGridBoostSpeedMultiplier);
            iniUtil.Set(SECTION_NAME, nameof(largeGridBoostTwr), largeGridBoostTwr);
            iniUtil.Set(SECTION_NAME, nameof(interdictionDuration), interdictionDuration);
            iniUtil.Set(SECTION_NAME, nameof(combatZoneRadius), combatZoneRadius);

            // Large grid atmo/ion based ships
            iniUtil.Set(SECTION_NAME, nameof(largeGridBaseWeight), largeGridBaseWeight);
            iniUtil.Set(SECTION_NAME, nameof(largeGridBaseTwr), largeGridBaseTwr);
            iniUtil.Set(SECTION_NAME, nameof(largeGridMinimumTwr), largeGridMinimumTwr);
            iniUtil.Set(SECTION_NAME, nameof(largeGridMaximumTwr), largeGridMaximumTwr);
            iniUtil.Set(SECTION_NAME, nameof(largeGridSpeedFactor), largeGridSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridWeightFactor), largeGridWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridBaseTurnRate), largeGridBaseTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(largeGridMinimumTurnRate), largeGridMinimumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(largeGridMaximumTurnRate), largeGridMaximumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(largeGridTurnRateWeightFactor), largeGridTurnRateWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridTurnRateSpeedFactor), largeGridTurnRateSpeedFactor);

            // Large grid gas based ships
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetBaseWeight), largeGridJetBaseWeight);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetBaseTwr), largeGridJetBaseTwr);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetMinimumTwr), largeGridJetMinimumTwr);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetMaximumTwr), largeGridJetMaximumTwr);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetSpeedFactor), largeGridJetSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetWeightFactor), largeGridJetWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetBaseTurnRate), largeGridJetBaseTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetMinimumTurnRate), largeGridJetMinimumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetMaximumTurnRate), largeGridJetMaximumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetTurnRateWeightFactor), largeGridJetTurnRateWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetTurnRateSpeedFactor), largeGridJetTurnRateSpeedFactor);

            // Small grid atmo/ion based ships
            iniUtil.Set(SECTION_NAME, nameof(smallGridBaseWeight), smallGridBaseWeight);
            iniUtil.Set(SECTION_NAME, nameof(smallGridBaseTwr), smallGridBaseTwr);
            iniUtil.Set(SECTION_NAME, nameof(smallGridMinimumTwr), smallGridMinimumTwr);
            iniUtil.Set(SECTION_NAME, nameof(smallGridMaximumTwr), smallGridMaximumTwr);
            iniUtil.Set(SECTION_NAME, nameof(smallGridSpeedFactor), smallGridSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridWeightFactor), smallGridWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridBaseTurnRate), smallGridBaseTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(smallGridMinimumTurnRate), smallGridMinimumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(smallGridMaximumTurnRate), smallGridMaximumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(smallGridTurnRateWeightFactor), smallGridTurnRateWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridTurnRateSpeedFactor), smallGridTurnRateSpeedFactor);

            // Small grid gas based ships
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetBaseWeight), smallGridJetBaseWeight);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetBaseTwr), smallGridJetBaseTwr);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetMinimumTwr), smallGridJetMinimumTwr);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetMaximumTwr), smallGridJetMaximumTwr);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetSpeedFactor), smallGridJetSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetWeightFactor), smallGridJetWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetBaseTurnRate), smallGridJetBaseTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetMinimumTurnRate), smallGridJetMinimumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetMaximumTurnRate), smallGridJetMaximumTurnRate);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetTurnRateWeightFactor), smallGridJetTurnRateWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetTurnRateSpeedFactor), smallGridJetTurnRateSpeedFactor);
        }

        public string GetAvailableSettings()
        {
            string availableSettings = "Displaying available settings.";
            return availableSettings;
        }

        public string GetCurrentSettings()
        {
            string currentSettings = "Please type /c<setting name> to view a specific setting.";
            return currentSettings;
        }

        public string GetSetting(string name)
        {
            switch (name)
            {
                // Global Settings
                case nameof(allowSuperCruise): return allowSuperCruise.ToString();
                case nameof(globalLargeGridMaxSpeed): return globalLargeGridMaxSpeed.ToString();
                case nameof(globalSmallGridMaxSpeed): return globalSmallGridMaxSpeed.ToString();
                case nameof(largeGridMaxSpeed): return largeGridMaxSpeed.ToString();
                case nameof(smallGridMaxSpeed): return smallGridMaxSpeed.ToString();
                case nameof(minimumGridVolume): return minimumGridVolume.ToString();

                // Boost and Combat
                case nameof(smallGridBoostSpeedMultiplier): return smallGridBoostSpeedMultiplier.ToString();
                case nameof(largeGridBoostSpeedMultiplier): return largeGridBoostSpeedMultiplier.ToString();
                case nameof(largeGridBoostTwr): return largeGridBoostTwr.ToString();
                case nameof(interdictionDuration): return interdictionDuration.ToString();
                case nameof(combatZoneRadius): return combatZoneRadius.ToString();

                // Large grid atmo/ion based ships
                case nameof(largeGridBaseWeight): return largeGridBaseWeight.ToString();
                case nameof(largeGridBaseTwr): return largeGridBaseTwr.ToString();
                case nameof(largeGridMinimumTwr): return largeGridMinimumTwr.ToString();
                case nameof(largeGridMaximumTwr): return largeGridMaximumTwr.ToString();
                case nameof(largeGridSpeedFactor): return largeGridSpeedFactor.ToString();
                case nameof(largeGridWeightFactor): return largeGridWeightFactor.ToString();
                case nameof(largeGridBaseTurnRate): return largeGridBaseTurnRate.ToString();
                case nameof(largeGridMinimumTurnRate): return largeGridMinimumTurnRate.ToString();
                case nameof(largeGridMaximumTurnRate): return largeGridMaximumTurnRate.ToString();
                case nameof(largeGridTurnRateWeightFactor): return largeGridTurnRateWeightFactor.ToString();
                case nameof(largeGridTurnRateSpeedFactor): return largeGridTurnRateSpeedFactor.ToString();

                // Large grid gas based ships
                case nameof(largeGridJetBaseWeight): return largeGridJetBaseWeight.ToString();
                case nameof(largeGridJetBaseTwr): return largeGridJetBaseTwr.ToString();
                case nameof(largeGridJetMinimumTwr): return largeGridJetMinimumTwr.ToString();
                case nameof(largeGridJetMaximumTwr): return largeGridJetMaximumTwr.ToString();
                case nameof(largeGridJetSpeedFactor): return largeGridJetSpeedFactor.ToString();
                case nameof(largeGridJetWeightFactor): return largeGridJetWeightFactor.ToString();
                case nameof(largeGridJetBaseTurnRate): return largeGridJetBaseTurnRate.ToString();
                case nameof(largeGridJetMinimumTurnRate): return largeGridJetMinimumTurnRate.ToString();
                case nameof(largeGridJetMaximumTurnRate): return largeGridJetMaximumTurnRate.ToString();
                case nameof(largeGridJetTurnRateWeightFactor): return largeGridJetTurnRateWeightFactor.ToString();
                case nameof(largeGridJetTurnRateSpeedFactor): return largeGridJetTurnRateSpeedFactor.ToString();

                // Small grid atmo/ion based ships
                case nameof(smallGridBaseWeight): return smallGridBaseWeight.ToString();
                case nameof(smallGridBaseTwr): return smallGridBaseTwr.ToString();
                case nameof(smallGridMinimumTwr): return smallGridMinimumTwr.ToString();
                case nameof(smallGridMaximumTwr): return smallGridMaximumTwr.ToString();
                case nameof(smallGridSpeedFactor): return smallGridSpeedFactor.ToString();
                case nameof(smallGridWeightFactor): return smallGridWeightFactor.ToString();
                case nameof(smallGridBaseTurnRate): return smallGridBaseTurnRate.ToString();
                case nameof(smallGridMinimumTurnRate): return smallGridMinimumTurnRate.ToString();
                case nameof(smallGridMaximumTurnRate): return smallGridMaximumTurnRate.ToString();
                case nameof(smallGridTurnRateWeightFactor): return smallGridTurnRateWeightFactor.ToString();
                case nameof(smallGridTurnRateSpeedFactor): return smallGridTurnRateSpeedFactor.ToString();

                // Small grid gas based ships
                case nameof(smallGridJetBaseWeight): return smallGridJetBaseWeight.ToString();
                case nameof(smallGridJetBaseTwr): return smallGridJetBaseTwr.ToString();
                case nameof(smallGridJetMinimumTwr): return smallGridJetMinimumTwr.ToString();
                case nameof(smallGridJetMaximumTwr): return smallGridJetMaximumTwr.ToString();
                case nameof(smallGridJetSpeedFactor): return smallGridJetSpeedFactor.ToString();
                case nameof(smallGridJetWeightFactor): return smallGridJetWeightFactor.ToString();
                case nameof(smallGridJetBaseTurnRate): return smallGridJetBaseTurnRate.ToString();
                case nameof(smallGridJetMinimumTurnRate): return smallGridJetMinimumTurnRate.ToString();
                case nameof(smallGridJetMaximumTurnRate): return smallGridJetMaximumTurnRate.ToString();
                case nameof(smallGridJetTurnRateWeightFactor): return smallGridJetTurnRateWeightFactor.ToString();
                case nameof(smallGridJetTurnRateSpeedFactor): return smallGridJetTurnRateSpeedFactor.ToString();

                default: return string.Empty;
            }
        }

        public SettingLimits GetLimits(string name)
        {
            return settingLimits[name];
        }

        public bool UpdateSetting(string name, string value)
        {
            bool result = false;
            if (settingLimits.ContainsKey(name))
            {
                SettingLimits limit = settingLimits[name];
                result = limit.IsValid(value);

                if (result)
                {
                    switch (name)
                    {
                        // Global Settings
                        case nameof(allowSuperCruise): allowSuperCruise = bool.Parse(value); break;
                        case nameof(globalLargeGridMaxSpeed): globalLargeGridMaxSpeed = float.Parse(value); break;
                        case nameof(globalSmallGridMaxSpeed): globalSmallGridMaxSpeed = float.Parse(value); break;
                        case nameof(largeGridMaxSpeed): largeGridMaxSpeed = float.Parse(value); break;
                        case nameof(smallGridMaxSpeed): smallGridMaxSpeed = float.Parse(value); break;
                        case nameof(minimumGridVolume): minimumGridVolume = float.Parse(value); break;

                        // Boost and Combat
                        case nameof(smallGridBoostSpeedMultiplier): smallGridBoostSpeedMultiplier = float.Parse(value); break;
                        case nameof(largeGridBoostSpeedMultiplier): largeGridBoostSpeedMultiplier = float.Parse(value); break;
                        case nameof(largeGridBoostTwr): largeGridBoostTwr = float.Parse(value); break;
                        case nameof(interdictionDuration): interdictionDuration = int.Parse(value); break;
                        case nameof(combatZoneRadius): combatZoneRadius = float.Parse(value); break;

                        // Large grid atmo/ion based ships
                        case nameof(largeGridBaseWeight): largeGridBaseWeight = float.Parse(value); break;
                        case nameof(largeGridBaseTwr): largeGridBaseTwr = float.Parse(value); break;
                        case nameof(largeGridMinimumTwr): largeGridMinimumTwr = float.Parse(value); break;
                        case nameof(largeGridMaximumTwr): largeGridMaximumTwr = float.Parse(value); break;
                        case nameof(largeGridSpeedFactor): largeGridSpeedFactor = float.Parse(value); break;
                        case nameof(largeGridWeightFactor): largeGridWeightFactor = float.Parse(value); break;
                        case nameof(largeGridBaseTurnRate): largeGridBaseTurnRate = float.Parse(value); break;
                        case nameof(largeGridMinimumTurnRate): largeGridMinimumTurnRate = float.Parse(value); break;
                        case nameof(largeGridMaximumTurnRate): largeGridMaximumTurnRate = float.Parse(value); break;
                        case nameof(largeGridTurnRateWeightFactor): largeGridTurnRateWeightFactor = float.Parse(value); break;
                        case nameof(largeGridTurnRateSpeedFactor): largeGridTurnRateSpeedFactor = float.Parse(value); break;

                        // Large grid gas based ships
                        case nameof(largeGridJetBaseWeight): largeGridJetBaseWeight = float.Parse(value); break;
                        case nameof(largeGridJetBaseTwr): largeGridJetBaseTwr = float.Parse(value); break;
                        case nameof(largeGridJetMinimumTwr): largeGridJetMinimumTwr = float.Parse(value); break;
                        case nameof(largeGridJetMaximumTwr): largeGridJetMaximumTwr = float.Parse(value); break;
                        case nameof(largeGridJetSpeedFactor): largeGridJetSpeedFactor = float.Parse(value); break;
                        case nameof(largeGridJetWeightFactor): largeGridJetWeightFactor = float.Parse(value); break;
                        case nameof(largeGridJetBaseTurnRate): largeGridJetBaseTurnRate = float.Parse(value); break;
                        case nameof(largeGridJetMinimumTurnRate): largeGridJetMinimumTurnRate = float.Parse(value); break;
                        case nameof(largeGridJetMaximumTurnRate): largeGridJetMaximumTurnRate = float.Parse(value); break;
                        case nameof(largeGridJetTurnRateWeightFactor): largeGridJetTurnRateWeightFactor = float.Parse(value); break;
                        case nameof(largeGridJetTurnRateSpeedFactor): largeGridJetTurnRateSpeedFactor = float.Parse(value); break;

                        // Small grid atmo/ion based ships
                        case nameof(smallGridBaseWeight): smallGridBaseWeight = float.Parse(value); break;
                        case nameof(smallGridBaseTwr): smallGridBaseTwr = float.Parse(value); break;
                        case nameof(smallGridMinimumTwr): smallGridMinimumTwr = float.Parse(value); break;
                        case nameof(smallGridMaximumTwr): smallGridMaximumTwr = float.Parse(value); break;
                        case nameof(smallGridSpeedFactor): smallGridSpeedFactor = float.Parse(value); break;
                        case nameof(smallGridWeightFactor): smallGridWeightFactor = float.Parse(value); break;
                        case nameof(smallGridBaseTurnRate): smallGridBaseTurnRate = float.Parse(value); break;
                        case nameof(smallGridMinimumTurnRate): smallGridMinimumTurnRate = float.Parse(value); break;
                        case nameof(smallGridMaximumTurnRate): smallGridMaximumTurnRate = float.Parse(value); break;
                        case nameof(smallGridTurnRateWeightFactor): smallGridTurnRateWeightFactor = float.Parse(value); break;
                        case nameof(smallGridTurnRateSpeedFactor): smallGridTurnRateSpeedFactor = float.Parse(value); break;

                        // Small grid gas based ships
                        case nameof(smallGridJetBaseWeight): smallGridJetBaseWeight = float.Parse(value); break;
                        case nameof(smallGridJetBaseTwr): smallGridJetBaseTwr = float.Parse(value); break;
                        case nameof(smallGridJetMinimumTwr): smallGridJetMinimumTwr = float.Parse(value); break;
                        case nameof(smallGridJetMaximumTwr): smallGridJetMaximumTwr = float.Parse(value); break;
                        case nameof(smallGridJetSpeedFactor): smallGridJetSpeedFactor = float.Parse(value); break;
                        case nameof(smallGridJetWeightFactor): smallGridJetWeightFactor = float.Parse(value); break;
                        case nameof(smallGridJetBaseTurnRate): smallGridJetBaseTurnRate = float.Parse(value); break;
                        case nameof(smallGridJetMinimumTurnRate): smallGridJetMinimumTurnRate = float.Parse(value); break;
                        case nameof(smallGridJetMaximumTurnRate): smallGridJetMaximumTurnRate = float.Parse(value); break;
                        case nameof(smallGridJetTurnRateWeightFactor): smallGridJetTurnRateWeightFactor = float.Parse(value); break;
                        case nameof(smallGridJetTurnRateSpeedFactor): smallGridJetTurnRateSpeedFactor = float.Parse(value); break;

                        default: return false;
                    }
                }
            }

            return result;
        }

        private void ShowParseError()
        {
            string message = "Error reading Ship Rudder Mod settings, settings have been reset.";
            CombatInterdictionSession.Instance.Messaging.NotifyPlayer(message);
            MyLog.Default.WriteLineAndConsole(message);
        }
    }

    public abstract class SettingLimits
    {
        public abstract bool IsValid(object value);
        protected bool IsWithinLimits(double value, double min, double max)
        {
            return (value >= min && value <= max);
        }
    }

    public class BoolLimits : SettingLimits
    {
        public bool DefaultValue;
        public override bool IsValid(object value)
        {
            bool boolValue;
            return bool.TryParse(value.ToString(), out boolValue);
        }
    }

    public class IntLimits : SettingLimits
    {
        public int DefaultValue;
        public int MinValue;
        public int MaxValue;

        public override bool IsValid(object value)
        {
            int intValue;
            if (int.TryParse(value.ToString(), out intValue))
            {
                if (IsWithinLimits(intValue, MinValue, MaxValue))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class FloatLimits : SettingLimits
    {
        public float DefaultValue;
        public float MinValue;
        public float MaxValue;

        public override bool IsValid(object value)
        {
            float floatValue;
            if (float.TryParse(value.ToString(), out floatValue))
            {
                if(IsWithinLimits(floatValue, MinValue, MaxValue))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class DoubleLimits : SettingLimits
    {
        public double DefaultValue;
        public float MinValue;
        public float MaxValue;

        public override bool IsValid(object value)
        {
            double doubleValue;
            if (double.TryParse(value.ToString(), out doubleValue))
            {
                if (IsWithinLimits(doubleValue, MinValue, MaxValue))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
