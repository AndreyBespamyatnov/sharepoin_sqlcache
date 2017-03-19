namespace Navicon.SP.Components.SqlCache.ViewInterpritator
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Xml.Linq;

    using DevExpress.Data;

    using Microsoft.SharePoint;

    using Navicon.SP.Components.GridDataEdit.Constants;
    using Navicon.SP.Components.GridDataEdit.DataSource.DataTableSource;
    using Navicon.SP.Components.GridDataEdit.XML;
    using Navicon.SP.Components.SqlCache.SpSync;
    using Navicon.SP.Components.SqlCache.ViewInterpritator.CamlModel;
    using Navicon.SP.GridDataEditControl;

    using Rule = Navicon.SP.Components.GridDataEdit.XML.Rule;

    /// <summary>
    ///     Структура настроек для объекта <see cref="ViewOverrider" />
    /// </summary>
    public struct ViewOverriderSettings
    {
        /// <summary>
        ///     Содержит объекную модель настроек для грида <see cref="GridDataEdit" />
        /// </summary>
        public HeadList GridSettings { get; set; }

        /// <summary>
        ///     Содержит настройки для запросов в SQL сервер. Может быть равен <see cref="string.Empty" />
        /// </summary>
        public string SqlFilters { get; set; }
    }

    /// <summary>
    ///     Позволяет создать на основе стандарной SPView настройки и данные для компонента <see cref="GridDataEdit" />,
    ///     копирует все настройки из родного отображения.
    ///     Должен использоваться кеш. В коробке список/библиотека должен быть помечен настройкой Cached="True"
    ///     Предпологается использовать на той же странице где расположена стандартная SPView. При этом стандартное отображение
    ///     скрывается, но не удаляется вебчасть со страницы.
    /// </summary>
    public sealed class ViewOverrider : IDisposable
    {
        private readonly CamlXmlView _camlXmlQuery;
        private readonly SPUser _currentUser;
        private readonly HeadList _head;
        private readonly bool _settingsAllreadyCreated;
        private readonly SPList _spList;
        private readonly SPView _spView;
        private readonly string _sqlFilterString;
        private readonly ISpSyncProvider _syncProvider;

        public const string SPUserTemplate = "'[{SP_USER_TEMPLATE}]'";
        private ViewOverrider(SPView spView, SPUser currentUser, HeadList head = null, string sqlFilterString = null)
        {
            this._spView = spView;

            SyncType syncType = SyncType.SpList;
            this._syncProvider = new SpSyncProvider(syncType);
            this._currentUser = currentUser;

            if (head != null)
            {
                this._head = head;
                this._sqlFilterString = sqlFilterString;
                this._settingsAllreadyCreated = true;
            }
            else
            {
                this._head = new HeadList
                {
                    Heads = new List<Head>
                    {
                        new Head
                        {
                            Position = "Top",
                            ColumnGroups = new List<ColumnGroup>
                            {
                                new ColumnGroup
                                {
                                    Hide = true
                                }
                            }
                        }
                    },
                    Options = new OptionList
                    {
                        Options = new List<Option>()
                    },
                    RowDisablingRules = new RowDisablingRulesList
                    {
                        DontShowLikeDisabled = true,
                        Rules = new List<Rule>
                        {
                            new Rule
                            {
                                FieldName = Constants.ColumnSpPrefix + "ID",
                                FieldType = "Number",
                                Operator = "GreaterThen",
                                FieldValue = "0"
                            }
                        }
                    }
                };

                this._camlXmlQuery = CamlXmlView.TryParse<CamlXmlView>(spView.SchemaXml);
            }
        }

        private ViewOverrider(SPList spList, SPQuery spQuery, SPUser currentUser)
        {
            SyncType syncType = SyncType.SpList;
            this._syncProvider = new SpSyncProvider(syncType);
            this._camlXmlQuery = CamlXmlView.TryParse<CamlXmlView>(spQuery.ViewXml);
            this._currentUser = currentUser;
            this._spList = spList;

            this._head = new HeadList
            {
                Heads = new List<Head>
                {
                    new Head
                    {
                        Position = "Top",
                        ColumnGroups = new List<ColumnGroup>
                        {
                            new ColumnGroup
                            {
                                Hide = true
                            }
                        }
                    }
                }
            };

            this._sqlFilterString = this.BuildSqlFilterString();
            this._settingsAllreadyCreated = true;
        }

        /// <summary>
        ///     Возвращает объект типа <see cref="DataTableSource" /> с данными. Исходя из фильтра SQL.
        /// </summary>
        /// <param name="spView">Объект <see cref="SPView" />, необходим для валидации полей на отображении и в списке.</param>
        /// <param name="currentUser">
        ///     Текущий полльзователь, объект <see cref="SPUser" />, используется для расчёта доступа к
        ///     элементам списка.
        /// </param>
        /// <param name="settings">Объект <see cref="ViewOverriderSettings" /> настроек.</param>
        /// <returns>Возвращает собранный объект <see cref="DataTableSource" /> с данными.</returns>
        public static DataTableSource GetDataTableBySettings(SPView spView, SPUser currentUser, ViewOverriderSettings settings)
        {
            string sqlFilters = settings.SqlFilters;
            if (SPContext.Current != null && SPContext.Current.Web != null && SPContext.Current.Web.CurrentUser != null)
            {
                string userValue = string.Format("'{0};#{1}'", SPContext.Current.Web.CurrentUser.ID, SPContext.Current.Web.CurrentUser.Name);
                sqlFilters = sqlFilters.Replace(ViewOverrider.SPUserTemplate, userValue);
            }
            ViewOverrider dataTableBySettings = new ViewOverrider(spView, currentUser, settings.GridSettings, sqlFilters);
            DataTableSource dataTableSource = dataTableBySettings.BuildDataTableSource();
            return dataTableSource;
        }

        /// <summary>
        ///     Возвращает объект типа <see cref="DataTable" /> с данными. Исходя из фильтра SQL.
        /// </summary>
        /// <param name="spList"></param>
        /// <param name="spQuery">Объект <see cref="SPQuery" />, необходим для енерации запроса в БД.</param>
        /// <param name="currentUser">
        ///     Текущий полльзователь, объект <see cref="SPUser" />, используется для расчёта доступа к
        ///     элементам списка.
        /// </param>
        /// <returns>Возвращает собранный объект <see cref="DataTable" /> с данными.</returns>
        public static DataTable GetDataTable(SPList spList, SPQuery spQuery, SPUser currentUser)
        {
            ViewOverrider overrider = new ViewOverrider(spList, spQuery, currentUser);
            DataTable dataTable = overrider.BuildDataTable();
            return dataTable;
        }

        /// <summary>
        ///     Позволяет сгенерировать объект настроек <see cref="ViewOverriderSettings" />, на основе объекта
        ///     <see cref="SPView" /> и полльзователя <see cref="SPUser" />.
        /// </summary>
        /// <param name="spView">Объект <see cref="SPView" />, необходим для генерации настроек.</param>
        /// <param name="currentUser">
        ///     Текущий полльзователь, объект <see cref="SPUser" />, используется для расчёта доступа к
        ///     элементам списка.
        /// </param>
        /// <returns>Объект <see cref="ViewOverriderSettings" />.</returns>
        public static ViewOverriderSettings CreateViewSettings(SPView spView, SPUser currentUser)
        {
            ViewOverrider viewOverrider = new ViewOverrider(spView, currentUser);
            string sqlFilterString;
            HeadList gridSettings = viewOverrider.BuildSettings(out sqlFilterString);
            ViewOverriderSettings settings = new ViewOverriderSettings
            {
                GridSettings = gridSettings,
                SqlFilters = sqlFilterString
            };
            
            return settings;
        }

        private DataTable BuildDataTable()
        {
            foreach (CamlXmlFieldRef fieldRef in this._camlXmlQuery.ViewFields.FieldRef)
            {
                if (!this._spList.Fields.ContainsField(fieldRef.Name))
                {
                    continue;
                }

                SPField spField = this._spList.Fields.GetField(fieldRef.Name);
                ColumnProperties column = new ColumnProperties
                {
                    FieldName = fieldRef.Name,
                    Caption = spField.Title,
                    Visible = true,
                    Type = spField.Type.ToString(),
                    GroupIndex = -1
                };
                this._head.Heads[0].ColumnGroups[0].Columns.Add(column);
            }

            DataTable dataTable = this.GenerateData(this._camlXmlQuery.RowLimit.Value);
            return dataTable;
        }

        private DataTableSource BuildDataTableSource()
        {
            DataTable dataTable = this.GenerateData();

            if (!this._settingsAllreadyCreated)
            {
                this.GenerateOrdering();
                this.GenerateGridSettings();
            }

            string xmlSettings = this._head.ToString();
            XElement gridSettings = XElement.Parse(xmlSettings);
            DataTableSource ds = new DataTableSource(dataTable, gridSettings);
            return ds;
        }

        private HeadList BuildSettings(out string sqlFilterString)
        {
            sqlFilterString = string.Empty;
            if (this._camlXmlQuery == null)
            {
                return null;
            }

            this.GenerateGridColumns();
            sqlFilterString = this.BuildSqlFilterString();
            this.GenerateOrdering();
            this.GenerateGridSettings();

            return this._head;
        }

        private string BuildSqlFilterString()
        {
            if (this._camlXmlQuery == null || this._camlXmlQuery.Query == null)
            {
                return string.Empty;
            }

            if (this._camlXmlQuery.Query.Where == null)
            {
                return string.Empty;
            }

            ILogicalOperators operatorOne = this._camlXmlQuery.Query.Where.LogicalOperatorOne;
            return GetSqlString(operatorOne);
        }

        private void GenerateOrdering()
        {
            if (this._camlXmlQuery.Query == null || this._camlXmlQuery.Query.OrderBy == null || this._camlXmlQuery.Query.OrderBy.FieldRef == null || this._camlXmlQuery.Query.OrderBy.FieldRef.Count == 0)
            {
                return;
            }

            List<CamlXmlFieldRef> camlXmlOrderBy = this._camlXmlQuery.Query.OrderBy.FieldRef;

            if (camlXmlOrderBy.Count > 0)
            {
                int count = 1;
                foreach (CamlXmlFieldRef field in camlXmlOrderBy)
                {
                    bool asc;
                    if(!bool.TryParse(field.Ascending, out asc))
                    {
                        asc = true;
                    }
                    this._head.Sort.SortFields.Add(new SortField() { Name = field.Name, Ascending = asc, SortOrder = count++ });
                }
            }
        }

        private void GenerateGridSettings()
        {
            CamlXmlRowLimit camlXmlRowLimit = this._camlXmlQuery.RowLimit; // TODO write to XML settings

            Option groupingOption = new Option
            {
                Name = Enums.OptionType.Grouping.ToString(),
                Enable = true
            };
            Option groupingValueOption = new Option
            {
                Name = Enums.OptionType.GroupingValue.ToString(),
                Enable = false
            };
            Option fileringOption = new Option
            {
                Name = Enums.OptionType.Filtering.ToString(),
                Enable = true
            };
            Option hideFilterBarOption = new Option
            {
                Name = Enums.OptionType.HideFilterBar.ToString(),
                Enable = false
            };
            Option forceSpViewClientContext = new Option
            {
                Name = Enums.OptionType.ForceSPViewClientContext.ToString(),
                Enable = true,
                FieldName = Constants.ColumnSpPrefix + "ID"
            };
            Option keyFieldName = new Option
            {
                Name = Enums.OptionType.KeyFieldName.ToString(),
                Value = Constants.ColumnSpPrefix + "ID"
            };
            Option expandAllGroups = new Option
            {
                Name = Enums.OptionType.ExpandAllGroups.ToString(),
                Enable = true
            };
            Option loadExcel = new Option
            {
                Name = Enums.OptionType.LoadExcel.ToString(),
                Enable = true
            };
            Option getFromTextFieldLookupTitle = new Option
            {
                Name = Enums.OptionType.GetFromTextFieldLookupTitle.ToString(),
                Enable = true
            };
            Option pageSize = new Option
            {
                Name = Enums.OptionType.PageSize.ToString(),
                Value = camlXmlRowLimit.Value.ToString("D")
            };
            Option showPaging = new Option
            {
                Name = Enums.OptionType.ShowPaging.ToString(),
                Enable = true,
                Value = "10,20,30,50,100,200"
            };
            Option enableMultiselect = new Option
            {
                Name = Enums.OptionType.EnableMultipleSelect.ToString(),
                Enable = true
            };
            Option showGroupPanel = new Option
            {
                Name = Enums.OptionType.ShowGroupPanel.ToString(),
                Enable = true
            };

            this._head.Options.Options.Add(groupingOption);
            this._head.Options.Options.Add(groupingValueOption);
            this._head.Options.Options.Add(fileringOption);
            this._head.Options.Options.Add(hideFilterBarOption);
            this._head.Options.Options.Add(forceSpViewClientContext);
            this._head.Options.Options.Add(keyFieldName);
            this._head.Options.Options.Add(expandAllGroups);
            this._head.Options.Options.Add(loadExcel);
            this._head.Options.Options.Add(getFromTextFieldLookupTitle);
            this._head.Options.Options.Add(pageSize);
            this._head.Options.Options.Add(showPaging);
            this._head.Options.Options.Add(enableMultiselect);
            this._head.Options.Options.Add(showGroupPanel);
        }

        private SummaryItemType GetSummaryType(string fieldRefType)
        {
            switch (fieldRefType.ToUpperInvariant())
            {
                case "AVG": // Среднее значение. Применяется к типам полей DateTimeNumber, Integer и Currency
                    return SummaryItemType.Average;
                case "COUNT": // Количество элементов. Применяется ко всем типам полей, позволяющим вычисления.
                    return SummaryItemType.Count;
                case "MAX": // Максимальное значение. Применяется к типам полей DateTimeNumber, Integer и Currency.
                    return SummaryItemType.Max;
                case "MIN": // Минимальное значение. Применяется к типам полей DateTimeNumber, Integer и Currency. 
                    return SummaryItemType.Min;
                case "SUM": //  Сумма значений. Применяется к типам полей Number, Integer и Currency.
                    return SummaryItemType.Sum;
                case "STDEV": // Стандартное отклонение. Применяется к типам полей Number, Integer и Currency.
                    return SummaryItemType.None;
                case "VAR": // Дисперсия. Применяется к типам полей Number, Integer и Currency.
                    return SummaryItemType.None;
                default:
                    return SummaryItemType.None;
            }
        }

        private void GenerateGridColumns()
        {
            if (this._camlXmlQuery.ViewFields == null)
            {
                return;
            }
            {
                if (this._head.Heads == null)
                {
                    this._head.Heads = new List<Head>();
                }
                Head settingsHeader = this._head.Heads.FirstOrDefault();

                if (settingsHeader == null)
                {
                    this._head.Heads.Clear();
                    this._head.Heads.Add(new Head
                    {
                        Position = "Top",
                        ColumnGroups = new List<ColumnGroup>
                        {
                            new ColumnGroup
                            {
                                Hide = true
                            }
                        }
                    });
                }
            }

            // а эта колонка нужна для работы клиентского контеста и рибона
            this._head.Heads[0].ColumnGroups[0].Columns.Add(new ColumnProperties
            {
                FieldName =  Constants.ColumnSpPrefix + "ID",
                Caption = "#",
                Visible = false,
                Type = SPFieldType.Text.ToString(),
                GroupIndex = -1
            });

            // а эта колонка нужна для работы клиентского контеста и рибона
            //this._head.Heads[0].ColumnGroups[0].Columns.Add(new ColumnProperties
            //{
            //    FieldName = "FullId",
            //    Caption = "#",
            //    Visible = false,
            //    Type = SPFieldType.Text.ToString(),
            //    GroupIndex = -1
            //});

            // а эта колонка нужна для ссылки на элемент
            bool linkIsPopupWindow = !this._spView.ParentList.NavigateForFormsPages;
            this._head.Heads[0].ColumnGroups[0].Columns.Add(new ColumnProperties
            {
                FieldName = "EditUrl",
                Caption = "#",
                Visible = true,
                Type = Enums.FieldType.AdvancedUrl.ToString(),
                NavigateUrlFormatString = "{0}",
                TextField = Constants.ColumnSpPrefix + "ID",
                // TextField = "FullId",
                LinkIsPopupWindow = linkIsPopupWindow,
                GroupIndex = -1
            });

            List<string> excludedFieldsList = new List<string> {"_CheckinComment", "_CopySource", "CheckoutUser", "AppEditor", "AppAuthor", "FileSizeDisplay", "DocIcon"};

            foreach (CamlXmlFieldRef fieldRef in this._camlXmlQuery.ViewFields.FieldRef)
            {
                bool isExcledudField = excludedFieldsList.Any(excludedFieldList => String.Equals(fieldRef.Name, excludedFieldList, StringComparison.InvariantCultureIgnoreCase));
                if (isExcledudField  || !this._spView.ParentList.Fields.ContainsField(fieldRef.Name))
                {
                    continue;
                }

                SPField spField = this._spView.ParentList.Fields.GetField(fieldRef.Name);
                ColumnProperties column = new ColumnProperties
                {
                    FieldName = fieldRef.Name,
                    Caption = spField.Title,
                    Visible = true,
                    Type = spField.Type.ToString(),
                    GroupIndex = -1
                };
                SPFieldNumber spFieldNumber = spField as SPFieldNumber;
                if (spFieldNumber != null && 
                    spFieldNumber.DisplayFormat != SPNumberFormatTypes.Automatic &&
                    spFieldNumber.DisplayFormat != SPNumberFormatTypes.NoDecimal)
                {
                    column.DisplayFormat = "N2";
                }
                if (this._camlXmlQuery.Query != null && this._camlXmlQuery.Query.GroupBy != null &&
                    this._camlXmlQuery.Query.GroupBy.FieldRef != null &&
                    this._camlXmlQuery.Query.GroupBy.FieldRef.Count != 0)
                {
                    if (this._camlXmlQuery.Query.GroupBy.FieldRef.Any(f => f.Name == fieldRef.Name))
                    {
                        int max = this._head.Heads[0].ColumnGroups[0].Columns.Max(c => c.GroupIndex);
                        column.GroupIndex = max == 0 ? 0 : max + 1;
                    }
                }

                if (this._camlXmlQuery.Aggregations != null && this._camlXmlQuery.Aggregations.Value == "On" &&
                    this._camlXmlQuery.Aggregations.FieldRef != null && this._camlXmlQuery.Aggregations.FieldRef.Count != 0)
                {
                    foreach (CamlXmlFieldRef camlXmlFieldRef in this._camlXmlQuery.Aggregations.FieldRef)
                    {
                        if (camlXmlFieldRef.Name == fieldRef.Name)
                        {
                            column.Summary = true;
                            column.SummaryType = this.GetSummaryType(camlXmlFieldRef.Type);
                            column.ShowSummary = 1;
                        }
                    }
                }

                this._head.Heads[0].ColumnGroups[0].Columns.Add(column);
            }
        }

        private DataTable GenerateData(long rowLimit = 0)
        {
            if (!this._settingsAllreadyCreated)
            {
                return new DataTable();
            }

            List<ColumnProperties> dataColumns = new List<ColumnProperties>();
            Head settingsHeader = this._head.Heads.FirstOrDefault();
            if (settingsHeader != null)
            {
                ColumnGroup settingsColumnGroup = settingsHeader.ColumnGroups.FirstOrDefault();
                if (settingsColumnGroup != null)
                {
                    dataColumns = settingsColumnGroup.Columns.ToList();
                }
            }

            SPList parentList = this._spView == null ? this._spList : this._spView.ParentList;
            List<int> userGroups = this._currentUser.Groups.Cast<SPGroup>().Select(g => g.ID).ToList();
            if (string.IsNullOrWhiteSpace(this._sqlFilterString))
            {
                DataTable tableData = this._syncProvider.GetItems(parentList, dataColumns, string.Empty, this._currentUser.ID, userGroups, rowLimit);
                return tableData;
            }
            else
            {
                DataTable tableData = this._syncProvider.GetItems(parentList, dataColumns, this._sqlFilterString, this._currentUser.ID, userGroups, rowLimit);
                return tableData;
            }
        }

        private static string GetSqlString(ILogicalOperators operatorOne)
        {
            const string tableNameFormat = Constants.SqlScriptTablePrefix + ".[{0}]";

            string sqlstring = "";
            string valueFormat;

            switch (operatorOne.OperatorName)
            {
                case OperatorType.And:
                    CamlXmlAnd camlXmlAnd = operatorOne as CamlXmlAnd;
                    if (camlXmlAnd == null)
                    {
                        break;
                    }

                    string sqlStringXmlAndOperatorOne = GetSqlString(camlXmlAnd.LogicalOperatorOne);
                    string sqlStringXmlAndOperatorTwo = GetSqlString(camlXmlAnd.LogicalOperatorTwo);

                    sqlstring += string.Format("({0} AND {1})", sqlStringXmlAndOperatorOne, sqlStringXmlAndOperatorTwo);
                    break;
                case OperatorType.Or:
                    CamlXmlOr camlXmlOr = operatorOne as CamlXmlOr;
                    if (camlXmlOr == null)
                    {
                        break;
                    }

                    string sqlStringXmlOrOperatorOne = GetSqlString(camlXmlOr.LogicalOperatorOne);
                    string sqlStringXmlOrOperatorTwo = GetSqlString(camlXmlOr.LogicalOperatorTwo);

                    sqlstring += string.Format("({0} OR {1})", sqlStringXmlOrOperatorOne, sqlStringXmlOrOperatorTwo);
                    break;

                case OperatorType.Membership:
                    // CamlXmlMembership camlXmlMembership = operatorOne as CamlXmlMembership;
                    throw new NotImplementedException();

                case OperatorType.DateRangesOverlap:
                    // CamlXmlDateRangesOverlap camlXmlDateRangesOverlap = operatorOne as CamlXmlDateRangesOverlap;
                    throw new NotImplementedException();

                    // IFieldRefValueBase START
                case OperatorType.BeginsWith:
                    IFieldRefValueBase camlXmlBeginsWith = operatorOne as IFieldRefValueBase;
                    if (camlXmlBeginsWith == null)
                    {
                        break;
                    }

                    valueFormat = GetSqlOperationFormatByFieldType(camlXmlBeginsWith.Value.Type);
                    sqlstring += string.Format(tableNameFormat + camlXmlBeginsWith.SqlOperator + valueFormat, camlXmlBeginsWith.FieldRef.Name, camlXmlBeginsWith.Value.Value + "%");
                    break;
                case OperatorType.Contains:
                case OperatorType.Includes:
                    IFieldRefValueBase camlXmlContainsIncludes = operatorOne as IFieldRefValueBase;
                    if (camlXmlContainsIncludes == null)
                    {
                        break;
                    }

                    valueFormat = GetSqlOperationFormatByFieldType(camlXmlContainsIncludes.Value.Type);
                    sqlstring += string.Format(tableNameFormat + camlXmlContainsIncludes.SqlOperator + valueFormat, camlXmlContainsIncludes.FieldRef.Name,
                        "%" + camlXmlContainsIncludes.Value.Value + "%");
                    break;

                case OperatorType.Eq:
                case OperatorType.Geq:
                case OperatorType.Gt:
                case OperatorType.IsNotNull:
                case OperatorType.IsNull:
                case OperatorType.Leq:
                case OperatorType.Lt:
                case OperatorType.Neq:
                    IFieldRefValueBase camlXmlFieldRefValueBase = operatorOne as IFieldRefValueBase;
                    if (camlXmlFieldRefValueBase == null)
                    {
                        break;
                    }

                    valueFormat = GetSqlOperationFormatByFieldType(camlXmlFieldRefValueBase.Value.Type);
                    string value = camlXmlFieldRefValueBase.Value.Value == null && camlXmlFieldRefValueBase.Value.UserID != null ? SPUserTemplate : camlXmlFieldRefValueBase.Value.Value;
                    sqlstring += string.Format(tableNameFormat + camlXmlFieldRefValueBase.SqlOperator + valueFormat, camlXmlFieldRefValueBase.FieldRef.Name, value);
                    break;
                    // IFieldRefValueBase END

                case OperatorType.In:
                    CamlXmlIn camlXmlIn = operatorOne as CamlXmlIn;
                    if (camlXmlIn == null)
                    {
                        break;
                    }

                    string inSqlString = string.Empty;
                    foreach (CamlXmlValue camlXmlValue in camlXmlIn.Values.Value)
                    {
                        if (!string.IsNullOrWhiteSpace(inSqlString))
                        {
                            inSqlString += " OR ";
                        }

                        inSqlString += string.Format("([{0}] LIKE '%{1}%')", camlXmlIn.FieldRef.Name, camlXmlValue.Value);
                    }
                    sqlstring += inSqlString;
                    break;

                case OperatorType.NotIncludes:
                    CamlXmlNotIncludes camlXmlNotIncludes = operatorOne as CamlXmlNotIncludes;
                    if (camlXmlNotIncludes == null)
                    {
                        break;
                    }

                    sqlstring += string.Format("NOT ([{0}] LIKE '%{1}%')", camlXmlNotIncludes.FieldRef.Name, camlXmlNotIncludes.Value);
                    break;
            }

            return sqlstring;
        }

        private static string GetSqlOperationFormatByFieldType(string valueType)
        {
            SPFieldType fieldType;
            string format;

            Enum.TryParse(valueType, true, out fieldType);
            switch (fieldType)
            {
                case SPFieldType.Text:
                    format = "'{1}'";
                    break;
                case SPFieldType.Integer:
                    format = "{1}";
                    break;

                default:
                    format = "'{1}'";
                    break;
            }

            return format;
        }

        #region Implementation of IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._syncProvider != null)
            {
                this._syncProvider.Dispose();
            }
        }
        #endregion
    }
}