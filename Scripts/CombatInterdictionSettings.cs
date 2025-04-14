using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;

namespace Khjin.CombatInterdiction
{
    public class CombatInterdictionSettings
    {
        private const string configversion = "1.0.3";
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

        // Per Grid Speed Scaling
        public float largeGridSpeedFactor;
        public float largeGridWeightFactor;
        public float largeGridJetSpeedFactor;
        public float largeGridJetWeightFactor;
        public float smallGridSpeedFactor;
        public float smallGridWeightFactor;
        public float smallGridJetSpeedFactor;
        public float smallGridJetWeightFactor;

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
                DefaultValue = 250.0f,
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
                DefaultValue = 3.5f,
                MinValue = 1.0f,
                MaxValue = 50.0f
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

            // Per Grid Speed Scaling
            settingLimits.Add(nameof(largeGridSpeedFactor), new FloatLimits()
            {
                DefaultValue = 5690f,
                MinValue = 0.01f,
                MaxValue = 50000f,
            });
            settingLimits.Add(nameof(largeGridWeightFactor), new FloatLimits()
            {
                DefaultValue = -0.219f,
                MinValue = -0.9999f,
                MaxValue = -0.0001f,
            });
            settingLimits.Add(nameof(largeGridJetSpeedFactor), new FloatLimits()
            {
                DefaultValue = 5690f,
                MinValue = 0.01f,
                MaxValue = 50000f,
            });
            settingLimits.Add(nameof(largeGridJetWeightFactor), new FloatLimits()
            {
                DefaultValue = -0.219f,
                MinValue = -0.9999f,
                MaxValue = -0.0001f,
            });
            settingLimits.Add(nameof(smallGridSpeedFactor), new FloatLimits()
            {
                DefaultValue = 375f,
                MinValue = 0.01f,
                MaxValue = 5000.0f,
            });
            settingLimits.Add(nameof(smallGridWeightFactor), new FloatLimits()
            {
                DefaultValue = -0.507f,
                MinValue = -0.9999f,
                MaxValue = -0.0001f,
            });
            settingLimits.Add(nameof(smallGridJetSpeedFactor), new FloatLimits()
            {
                DefaultValue = 3150f,
                MinValue = 0.01f,
                MaxValue = 5000.0f,
            });
            settingLimits.Add(nameof(smallGridJetWeightFactor), new FloatLimits()
            {
                DefaultValue = -0.657f,
                MinValue = -0.9999f,
                MaxValue = -0.0001f,
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

            // Per Grid Speed Scaling
            largeGridSpeedFactor = ((FloatLimits)settingLimits[nameof(largeGridSpeedFactor)]).DefaultValue;
            largeGridWeightFactor = ((FloatLimits)settingLimits[nameof(largeGridWeightFactor)]).DefaultValue;
            smallGridSpeedFactor = ((FloatLimits)settingLimits[nameof(smallGridSpeedFactor)]).DefaultValue;
            smallGridWeightFactor = ((FloatLimits)settingLimits[nameof(smallGridWeightFactor)]).DefaultValue;
            largeGridJetSpeedFactor = ((FloatLimits)settingLimits[nameof(largeGridJetSpeedFactor)]).DefaultValue;
            largeGridJetWeightFactor = ((FloatLimits)settingLimits[nameof(largeGridJetWeightFactor)]).DefaultValue;
            smallGridJetSpeedFactor = ((FloatLimits)settingLimits[nameof(smallGridJetSpeedFactor)]).DefaultValue;
            smallGridJetWeightFactor = ((FloatLimits)settingLimits[nameof(smallGridJetWeightFactor)]).DefaultValue;
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

            // Per Grid Speed Scaling
            largeGridSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridSpeedFactor)).ToDouble();
            largeGridWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridWeightFactor)).ToDouble();
            smallGridSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridSpeedFactor)).ToDouble();
            smallGridWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridWeightFactor)).ToDouble();
            largeGridJetSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetSpeedFactor)).ToDouble();
            largeGridJetWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(largeGridJetWeightFactor)).ToDouble();
            smallGridJetSpeedFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetSpeedFactor)).ToDouble();
            smallGridJetWeightFactor = (float)iniUtil.Get(SECTION_NAME, nameof(smallGridJetWeightFactor)).ToDouble();
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

            // Per Grid Speed Scaling
            iniUtil.Set(SECTION_NAME, nameof(largeGridSpeedFactor), largeGridSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridWeightFactor), largeGridWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridSpeedFactor), smallGridSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridWeightFactor), smallGridWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetSpeedFactor), largeGridJetSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(largeGridJetWeightFactor), largeGridJetWeightFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetSpeedFactor), smallGridJetSpeedFactor);
            iniUtil.Set(SECTION_NAME, nameof(smallGridJetWeightFactor), smallGridJetWeightFactor);
        }

        public string GetAvailableSettings()
        {
            string availableSettings =

            // Global Settings
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(allowSuperCruise)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(globalLargeGridMaxSpeed)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(globalSmallGridMaxSpeed)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(largeGridMaxSpeed)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(smallGridMaxSpeed)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(minimumGridVolume)}, " +

            // Boost and Combat
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(smallGridBoostSpeedMultiplier)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(largeGridBoostSpeedMultiplier)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(largeGridBoostTwr)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(interdictionDuration)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(combatZoneRadius)}, " +

            // Per Grid Speed Scaling
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(largeGridSpeedFactor)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(largeGridWeightFactor)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(largeGridJetSpeedFactor)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(largeGridJetWeightFactor)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(smallGridSpeedFactor)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(smallGridWeightFactor)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(smallGridJetSpeedFactor)}, " +
            $"{CombatInterdictionCommands.COMMAND_PREFIX}{nameof(smallGridJetWeightFactor)}";

            return availableSettings;
        }

        public string GetCurrentSettings()
        {
            string currentSettings =

                // Global Settings
                $"{nameof(allowSuperCruise)}={allowSuperCruise}, " +
                $"{nameof(globalLargeGridMaxSpeed)}={globalLargeGridMaxSpeed}, " +
                $"{nameof(globalSmallGridMaxSpeed)}={globalSmallGridMaxSpeed}, " +
                $"{nameof(largeGridMaxSpeed)}={largeGridMaxSpeed}, " +
                $"{nameof(smallGridMaxSpeed)}={smallGridMaxSpeed}, " +
                $"{nameof(minimumGridVolume)}={minimumGridVolume}, " +

                // Boost and Combat
                $"{nameof(smallGridBoostSpeedMultiplier)}={smallGridBoostSpeedMultiplier}, " +
                $"{nameof(largeGridBoostSpeedMultiplier)}={largeGridBoostSpeedMultiplier}, " +
                $"{nameof(largeGridBoostTwr)}={largeGridBoostTwr}, " +
                $"{nameof(interdictionDuration)}={interdictionDuration}, " +
                $"{nameof(combatZoneRadius)}={combatZoneRadius}, " +

                // Environment and Additional Factors
                $"{nameof(largeGridSpeedFactor)}={largeGridSpeedFactor}, " +
                $"{nameof(largeGridWeightFactor)}={largeGridWeightFactor}, " +
                $"{nameof(largeGridJetSpeedFactor)}={largeGridJetSpeedFactor}, " +
                $"{nameof(largeGridJetWeightFactor)}={largeGridJetWeightFactor}, " +
                $"{nameof(smallGridSpeedFactor)}={smallGridSpeedFactor}, " +
                $"{nameof(smallGridWeightFactor)}={smallGridWeightFactor}, " +
                $"{nameof(smallGridJetSpeedFactor)}={smallGridJetSpeedFactor}, " +
                $"{nameof(smallGridJetWeightFactor)}={smallGridJetWeightFactor}";

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

                // Per Grid Speed Scaling
                case nameof(largeGridSpeedFactor): return largeGridSpeedFactor.ToString();
                case nameof(largeGridWeightFactor): return largeGridWeightFactor.ToString();
                case nameof(largeGridJetSpeedFactor): return largeGridJetSpeedFactor.ToString();
                case nameof(largeGridJetWeightFactor): return largeGridJetWeightFactor.ToString();
                case nameof(smallGridSpeedFactor): return smallGridSpeedFactor.ToString();
                case nameof(smallGridWeightFactor): return smallGridWeightFactor.ToString();
                case nameof(smallGridJetSpeedFactor): return smallGridJetSpeedFactor.ToString();
                case nameof(smallGridJetWeightFactor): return smallGridJetWeightFactor.ToString();
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

                        // Per Grid Speed Scaling
                        case nameof(largeGridSpeedFactor): largeGridSpeedFactor = float.Parse(value); break;
                        case nameof(largeGridWeightFactor): largeGridWeightFactor = float.Parse(value); break;
                        case nameof(largeGridJetSpeedFactor): largeGridJetSpeedFactor = float.Parse(value); break;
                        case nameof(largeGridJetWeightFactor): largeGridJetWeightFactor = float.Parse(value); break;
                        case nameof(smallGridSpeedFactor): smallGridSpeedFactor = float.Parse(value); break;
                        case nameof(smallGridWeightFactor): smallGridWeightFactor = float.Parse(value); break;
                        case nameof(smallGridJetSpeedFactor): smallGridJetSpeedFactor = float.Parse(value); break;
                        case nameof(smallGridJetWeightFactor): smallGridJetWeightFactor = float.Parse(value); break;
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
