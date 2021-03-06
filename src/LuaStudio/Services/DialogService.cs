﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LuaStudio.Services
{

    /// <summary>
    /// Dialog service implementation
    /// </summary>
    [Export(typeof(IDialogService))]
    class DialogService : IDialogService
    {
        /// <summary>
        /// Display a selector to open a file
        /// </summary>
        public IEnumerable<String> FileOpen(
            String title,
            String defaultPath = null,
            bool multiSelect = false,
            bool acceptAllFileTypes = true
            )
        {
            var dlg = new OpenFileDialog();

            dlg.Title = title;
            dlg.Multiselect = multiSelect;
            if (!String.IsNullOrWhiteSpace(defaultPath))
                dlg.InitialDirectory = defaultPath;
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;

            // Extract filters from text editor definition
            StringBuilder sb = new StringBuilder();
            foreach (var def in AppContext.Current.GetTextDefinitions())
            {
                String[] exts = def.Extensions.Split(',', ';').Select(e => "*" + e).ToArray();
                String filters = String.Join(";", exts);
                if (sb.Length > 0) sb.Append('|');
                sb.AppendFormat("{0} ({1})|{1}", def.Caption, filters);
            }
            if (acceptAllFileTypes)
            {
                if (sb.Length > 0) sb.Append('|');
                sb.AppendFormat("{0} (*.*)|*.*", Resources.Locales.OpenFileDialog_AllTypesFiles_Caption);
            }
            dlg.Filter = sb.ToString();

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                return dlg.FileNames;
            }
            else
            {
                return Enumerable.Empty<String>();
            }
        }

        /// <summary>
        /// Display a selector to save a file
        /// </summary>
        public String FileSave(
            String title,
            String defaultFilename = null,
            String defaultPath = null,
            TextEditors.ITextDefinition definition = null,
            bool acceptAllFileTypes = true
            )
        {
            var dlg = new OpenFileDialog();

            dlg.Title = title;
            dlg.Multiselect = false;
            if (!String.IsNullOrWhiteSpace(defaultFilename))
            {
                dlg.FileName = defaultFilename;
                dlg.InitialDirectory = Path.GetDirectoryName(defaultFilename);
            }
            else if (!String.IsNullOrWhiteSpace(defaultPath))
                dlg.InitialDirectory = defaultPath;
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = true;

            // Extract filters from text editor definition
            StringBuilder sb = new StringBuilder();
            int idx = 0;
            foreach (var def in AppContext.Current.GetTextDefinitions())
            {
                String[] fexts = def.Extensions.Split(',', ';');
                String[] exts = fexts.Select(e => "*" + e).ToArray();
                String filters = String.Join(";", exts);
                if (sb.Length > 0) sb.Append('|');
                sb.AppendFormat("{0} ({1})|{1}", def.Caption, filters);
                if (def == definition)
                {
                    dlg.FilterIndex = idx;
                    dlg.DefaultExt = fexts.FirstOrDefault();
                }
                idx++;
            }
            if (acceptAllFileTypes)
            {
                if (sb.Length > 0) sb.Append('|');
                sb.AppendFormat("{0} (*.*)|*.*", Resources.Locales.OpenFileDialog_AllTypesFiles_Caption);
            }
            dlg.Filter = sb.ToString();

            if (dlg.ShowDialog().GetValueOrDefault())
            {
                return dlg.FileName;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Open a confirm dialog box
        /// </summary>
        public IDialogButton Confirm(
            String message,
            String title,
            params IDialogButton[] buttons
            )
        {
            return (IDialogButton)App.Current.Dispatcher.Invoke((Func<IDialogButton>)delegate
            {
                var confirm = new Dialogs.ConfirmDialog();
                var vm = new Dialogs.ConfirmDialogViewModel
                {
                    Title = title,
                    Message = message,
                    Buttons = buttons
                };
                confirm.DataContext = vm;
                confirm.ShowDialog();
                return vm.LastClickedButton;
            });
        }

        public bool DisplayError(String title, Exception error)
        {
            if (error != null)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    var confirm = new Dialogs.ConfirmDialog();
                    var vm = new Dialogs.ConfirmDialogViewModel
                    {
                        Title = title,
                        Message = error.GetBaseException().Message,
                        Buttons = new IDialogButton[] { DialogButton.OkButton() }
                    };
                    confirm.DataContext = vm;
                    confirm.ShowDialog();
                });
                return true;
            }
            return false;
        }

    }

}
