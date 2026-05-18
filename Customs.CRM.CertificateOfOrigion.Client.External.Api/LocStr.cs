	using System;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Markup;
	using Customs.Inf.MMI.Common.CAL;
	using Customs.Inf.MMI.Common.Extensions;

	namespace Customs.CRM.CertificateOfOrigins.Client.Api
	{
		/// <summary>
		/// XAML Extension Localized Strings
		/// </summary>
		[ValueConversion(typeof(string), typeof(string))]
		[MarkupExtensionReturnType(typeof (string))]
		public class LocStr : Inf.MMI.Common.Extensions.LocStr
		{
			private const string LocalDicPath =
						@"pack://application:,,,/Customs.CRM.CertificateOfOrigins.Client.Api;Component/LocalStrings.xaml";

			private const string EnglishDicPath  =
						@"pack://application:,,,/Customs.CRM.CertificateOfOrigins.Client.Api;Component/EnglishStrings.xaml";

			/// <summary>
			/// Initializes the <see cref="LocStr"/> class.
			/// </summary>
			static LocStr()
			{
                if (DependencyObjectExtentions.IsDesignMode)
                {
                    InitResourceDictionaries(LocalDicPath, EnglishDicPath);
                }
			}

		}
	}
