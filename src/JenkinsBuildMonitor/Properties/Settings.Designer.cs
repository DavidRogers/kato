﻿//------------------------------------------------------------------------------
// <auto-generated>
//	 This code was generated by a tool.
//	 Runtime Version:4.0.30319.34003
//
//	 Changes to this file may cause incorrect behavior and will be lost if
//	 the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Kato.Properties {
	
	
	[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
	internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
		
		private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
		
		public static Settings Default {
			get {
				return defaultInstance;
			}
		}
		
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public global::System.Collections.Specialized.StringCollection Servers {
			get {
				return ((global::System.Collections.Specialized.StringCollection)(this["Servers"]));
			}
			set {
				this["Servers"] = value;
			}
		}
		
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("True")]
		public bool AutoInstallUpdates {
			get {
				return ((bool)(this["AutoInstallUpdates"]));
			}
			set {
				this["AutoInstallUpdates"] = value;
			}
		}
		
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		[global::System.Configuration.DefaultSettingValueAttribute("List")]
		public string ViewMode {
			get {
				return ((string)(this["ViewMode"]));
			}
			set {
				this["ViewMode"] = value;
			}
		}
	}
}
