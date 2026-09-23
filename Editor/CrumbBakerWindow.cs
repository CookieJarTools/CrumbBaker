using System.Collections.Generic;
using System.IO;
using CookieJarTools.CrumbBaker.Core;
using CookieJarTools.CrumbBaker.Core.Rules;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CookieJarTools.CrumbBaker.Editor
{
    public sealed class CrumbBakerWindow : EditorWindow
    {
        [SerializeField]
        private StyleSheet styleSheet;

        private enum RuleSourceMode
        {
            ConfigFile,
            ManualRules
        }

        [SerializeField]
        private string packageRootPath;

        [SerializeField]
        private string configFilePath;

        [SerializeField]
        private RuleSourceMode ruleSourceMode = RuleSourceMode.ConfigFile;

        [System.Serializable]
        private sealed class EditableRule
        {
            public string Pattern = "package.json";
            public FileRequirementLevel RequirementLevel = FileRequirementLevel.Required;
            public string Description = "Package manifest";
            public string HandlerTypeName = string.Empty;
        }

        [SerializeField]
        private List<EditableRule> editableRules = new List<EditableRule>();

        [SerializeField]
        private Vector2 issuesScrollPosition;

        private Label summaryLabel;
        private ScrollView issuesScrollView;
        private RadioButton configFileModeRadio;
        private RadioButton manualRulesModeRadio;
        private VisualElement configFileSection;
        private VisualElement manualRulesSection;
        private ScrollView manualRulesScrollView;

        [MenuItem("Window/CookieJarTools/Crumb Baker")]
        private static void Open()
        {
            var window = GetWindow<CrumbBakerWindow>();
            window.titleContent = new GUIContent("Crumb Baker");
            window.minSize = new Vector2(420f, 380f);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            root.AddToClassList("crumbbaker-root");

            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }

            var header = new Label("Crumb Baker");
            header.AddToClassList("crumbbaker-header");
            root.Add(header);

            var packageSection = BuildPackageSection();
            root.Add(packageSection);

            var modeSection = BuildModeSection();
            root.Add(modeSection);

            configFileSection = BuildConfigFileSection();
            root.Add(configFileSection);

            manualRulesSection = BuildManualRulesSection();
            root.Add(manualRulesSection);
            UpdateModeVisibility();

            var resultsSection = BuildResultsSection();
            root.Add(resultsSection);

            var bottomBar = BuildBottomBar();
            root.Add(bottomBar);
        }

        private VisualElement BuildPackageSection()
        {
            var section = new VisualElement();
            section.AddToClassList("crumbbaker-section");

            var title = new Label("Package");
            title.AddToClassList("crumbbaker-section-title");
            section.Add(title);

            var row = new VisualElement();
            row.AddToClassList("crumbbaker-row-horizontal");

            var pathField = new TextField("Package Root");
            pathField.value = packageRootPath;
            pathField.AddToClassList("crumbbaker-text-field");
            pathField.RegisterValueChangedCallback(evt =>
            {
                packageRootPath = evt.newValue;
            });
            row.Add(pathField);

            var browseButton = new Button(() =>
            {
                var initial = string.IsNullOrEmpty(packageRootPath) ? Application.dataPath : packageRootPath;
                var selected = EditorUtility.OpenFolderPanel("Select Package Root", initial, string.Empty);
                if (!string.IsNullOrEmpty(selected))
                {
                    packageRootPath = selected;
                    pathField.SetValueWithoutNotify(packageRootPath);
                }
            })
            {
                text = "Browse"
            };
            browseButton.AddToClassList("crumbbaker-button-secondary");
            row.Add(browseButton);

            section.Add(row);
            return section;
        }

        private VisualElement BuildModeSection()
        {
            var section = new VisualElement();
            section.AddToClassList("crumbbaker-section");

            var title = new Label("Rule Source");
            title.AddToClassList("crumbbaker-section-title");
            section.Add(title);

            var modeRow = new VisualElement();
            modeRow.AddToClassList("crumbbaker-row-vertical");

            configFileModeRadio = new RadioButton("Config File (.crumbbaker.json)");
            configFileModeRadio.value = ruleSourceMode == RuleSourceMode.ConfigFile;
            configFileModeRadio.RegisterValueChangedCallback(evt =>
            {
                if (!evt.newValue)
                {
                    return;
                }

                ruleSourceMode = RuleSourceMode.ConfigFile;
                manualRulesModeRadio.SetValueWithoutNotify(false);
                UpdateModeVisibility();
            });
            modeRow.Add(configFileModeRadio);

            manualRulesModeRadio = new RadioButton("Manual Rules");
            manualRulesModeRadio.value = ruleSourceMode == RuleSourceMode.ManualRules;
            manualRulesModeRadio.RegisterValueChangedCallback(evt =>
            {
                if (!evt.newValue)
                {
                    return;
                }

                ruleSourceMode = RuleSourceMode.ManualRules;
                configFileModeRadio.SetValueWithoutNotify(false);
                UpdateModeVisibility();
            });
            modeRow.Add(manualRulesModeRadio);

            section.Add(modeRow);
            return section;
        }

        private VisualElement BuildConfigFileSection()
        {
            var section = new VisualElement();
            section.AddToClassList("crumbbaker-section");
            section.AddToClassList("crumbbaker-section-config");

            var title = new Label("Config File");
            title.AddToClassList("crumbbaker-section-subtitle");
            section.Add(title);

            var row = new VisualElement();
            row.AddToClassList("crumbbaker-row-horizontal");

            var configField = new TextField("Config Path");
            configField.value = configFilePath;
            configField.AddToClassList("crumbbaker-text-field");
            configField.RegisterValueChangedCallback(evt =>
            {
                configFilePath = evt.newValue;
            });
            row.Add(configField);

            var browseButton = new Button(() =>
            {
                var initial = string.IsNullOrEmpty(configFilePath) ? Application.dataPath : Path.GetDirectoryName(configFilePath);
                if (string.IsNullOrEmpty(initial))
                {
                    initial = Application.dataPath;
                }

                var selected = EditorUtility.OpenFilePanel("Select .crumbbaker.json", initial, "json");
                if (!string.IsNullOrEmpty(selected))
                {
                    configFilePath = selected;
                    configField.SetValueWithoutNotify(configFilePath);
                }
            })
            {
                text = "Browse"
            };
            browseButton.AddToClassList("crumbbaker-button-secondary");
            row.Add(browseButton);

            section.Add(row);
            return section;
        }

        private VisualElement BuildManualRulesSection()
        {
            var section = new VisualElement();
            section.AddToClassList("crumbbaker-section");
            section.AddToClassList("crumbbaker-section-rules");

            var titleRow = new VisualElement();
            titleRow.AddToClassList("crumbbaker-row-horizontal");

            var title = new Label("Manual Rules");
            title.AddToClassList("crumbbaker-section-subtitle");
            titleRow.Add(title);

            var addButton = new Button(AddEditableRule)
            {
                text = "+ Add Rule"
            };
            addButton.AddToClassList("crumbbaker-button-secondary");
            titleRow.Add(addButton);

            section.Add(titleRow);
            
            manualRulesScrollView  = new ScrollView(ScrollViewMode.Vertical);
            manualRulesScrollView .AddToClassList("crumbbaker-rules-scroll");
            section.Add(manualRulesScrollView );

            if (editableRules.Count == 0)
            {
                var defaultRule = new EditableRule();
                editableRules.Add(defaultRule);
            }

            RebuildRulesUI(manualRulesScrollView );
            return section;
        }
        
        private void AddEditableRule()
        {
            var newRule = new EditableRule();
            editableRules.Add(newRule);

            if (manualRulesScrollView != null)
            {
                RebuildRulesUI(manualRulesScrollView);
            }
        }

        private void RebuildRulesUI(ScrollView rulesScroll)
        {
            rulesScroll.Clear();

            for (var index = 0; index < editableRules.Count; index++)
            {
                var editableRule = editableRules[index];

                var ruleCard = new VisualElement();
                ruleCard.AddToClassList("crumbbaker-rule-card");

                var headerRow = new VisualElement();
                headerRow.AddToClassList("crumbbaker-row-horizontal");

                var patternField = new TextField("Pattern");
                patternField.value = editableRule.Pattern;
                patternField.AddToClassList("crumbbaker-text-field");
                patternField.RegisterValueChangedCallback(evt =>
                {
                    editableRule.Pattern = evt.newValue;
                });
                headerRow.Add(patternField);

                var requirementField = new EnumField(editableRule.RequirementLevel);
                requirementField.label = "Level";
                requirementField.AddToClassList("crumbbaker-enum-field");
                requirementField.RegisterValueChangedCallback(evt =>
                {
                    editableRule.RequirementLevel = (FileRequirementLevel)evt.newValue;
                });
                headerRow.Add(requirementField);

                var removeButton = new Button(() =>
                {
                    editableRules.Remove(editableRule);
                    RebuildRulesUI(rulesScroll);
                })
                {
                    text = "X"
                };
                removeButton.AddToClassList("crumbbaker-button-danger");
                headerRow.Add(removeButton);

                ruleCard.Add(headerRow);

                var descriptionField = new TextField("Description");
                descriptionField.value = editableRule.Description;
                descriptionField.AddToClassList("crumbbaker-text-field");
                descriptionField.RegisterValueChangedCallback(evt =>
                {
                    editableRule.Description = evt.newValue;
                });
                ruleCard.Add(descriptionField);

                var handlerField = new TextField("Handler Type (optional)");
                handlerField.value = editableRule.HandlerTypeName;
                handlerField.AddToClassList("crumbbaker-text-field");
                handlerField.tooltip = "Assembly-qualified type name implementing IFileRuleHandler.";
                handlerField.RegisterValueChangedCallback(evt =>
                {
                    editableRule.HandlerTypeName = evt.newValue;
                });
                ruleCard.Add(handlerField);

                rulesScroll.Add(ruleCard);
            }
        }

        private VisualElement BuildResultsSection()
        {
            var section = new VisualElement();
            section.AddToClassList("crumbbaker-section");

            var title = new Label("Results");
            title.AddToClassList("crumbbaker-section-title");
            section.Add(title);

            summaryLabel = new Label("No checks have been run yet.");
            summaryLabel.AddToClassList("crumbbaker-summary-label");
            section.Add(summaryLabel);

            issuesScrollView = new ScrollView(ScrollViewMode.Vertical);
            issuesScrollView.AddToClassList("crumbbaker-issues-scroll");
            section.Add(issuesScrollView);

            return section;
        }

        private VisualElement BuildBottomBar()
        {
            var bar = new VisualElement();
            bar.AddToClassList("crumbbaker-bottom-bar");

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1f;
            bar.Add(spacer);

            var runButton = new Button(RunChecks)
            {
                text = "Run Checks"
            };
            runButton.AddToClassList("crumbbaker-button-primary");
            bar.Add(runButton);

            return bar;
        }

        private void RunChecks()
        {
            if (string.IsNullOrWhiteSpace(packageRootPath) || !Directory.Exists(packageRootPath))
            {
                EditorUtility.DisplayDialog(
                    "Crumb Baker",
                    "Please select a valid package root directory.",
                    "OK");
                return;
            }

            PackageCheckResult result;

            if (ruleSourceMode == RuleSourceMode.ConfigFile)
            {
                if (string.IsNullOrWhiteSpace(configFilePath) || !File.Exists(configFilePath))
                {
                    EditorUtility.DisplayDialog(
                        "Crumb Baker",
                        "Please select a valid .crumbbaker.json config file.",
                        "OK");
                    return;
                }

                result = PackageValidator.ValidatePackage(packageRootPath, configFilePath);
            }
            else
            {
                var rulesList = new List<FileRule>(editableRules.Count);
                foreach (var editableRule in editableRules)
                {
                    if (string.IsNullOrWhiteSpace(editableRule.Pattern))
                    {
                        continue;
                    }

                    var rule = new FileRule(
                        editableRule.Pattern,
                        editableRule.RequirementLevel,
                        editableRule.Description,
                        editableRule.HandlerTypeName);
                    rulesList.Add(rule);
                }

                var rulesArray = rulesList.ToArray();
                result = PackageValidator.ValidatePackage(packageRootPath, rulesArray);
            }

            RenderResult(result);
        }

        private void RenderResult(PackageCheckResult result)
        {
            var errorsCount = result.Errors.Count;
            var warningsCount = result.Warnings.Count;

            if (errorsCount == 0 && warningsCount == 0)
            {
                summaryLabel.text = "No issues found.";
                summaryLabel.AddToClassList("crumbbaker-summary-ok");
            }
            else if (errorsCount == 0)
            {
                summaryLabel.text = $"No errors. {warningsCount} warning(s).";
                summaryLabel.RemoveFromClassList("crumbbaker-summary-ok");
            }
            else
            {
                summaryLabel.text = $"{errorsCount} error(s), {warningsCount} warning(s).";
                summaryLabel.RemoveFromClassList("crumbbaker-summary-ok");
            }

            issuesScrollView.Clear();

            foreach (var error in result.Errors)
            {
                var issueElement = new Label(error.ToString());
                issueElement.AddToClassList("crumbbaker-issue");
                issueElement.AddToClassList("crumbbaker-issue-error");
                issuesScrollView.Add(issueElement);
            }

            foreach (var warning in result.Warnings)
            {
                var issueElement = new Label(warning.ToString());
                issueElement.AddToClassList("crumbbaker-issue");
                issueElement.AddToClassList("crumbbaker-issue-warning");
                issuesScrollView.Add(issueElement);
            }
        }

        private void UpdateModeVisibility()
        {
            if (configFileSection != null)
            {
                configFileSection.style.display = ruleSourceMode == RuleSourceMode.ConfigFile
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            if (manualRulesSection != null)
            {
                manualRulesSection.style.display = ruleSourceMode == RuleSourceMode.ManualRules
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }
        }
    }
}