using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace JewelryStore
{
    public class show_all_people : Form
    {
        private readonly Color background =
            Color.FromArgb(244, 245, 247);

        private readonly Color accent =
            Color.FromArgb(48, 112, 225);

        private readonly Color textColor =
            Color.FromArgb(45, 47, 53);

        private readonly DataGridView peopleGrid = new();

        private readonly ComboBox cityFilter = new();
        private readonly ComboBox branchFilter = new();
        private readonly ComboBox divisionFilter = new();

        private readonly TextBox searchBox = new();

        private readonly Label countLabel = new();
        private readonly Label salesLabel = new();

        private readonly Button exportPdfButton = new();

        private readonly List<ClientRecord> clients = new();

        public show_all_people()
        {
            Text = "JewelryStore — All People";

            StartPosition = FormStartPosition.CenterScreen;

            MinimumSize = new Size(1150, 720);
            Size = new Size(1380, 850);

            BackColor = background;
            Font = new Font("Segoe UI", 9F);

            QuestPDF.Settings.License = LicenseType.Community;

            AddSampleClients();

            BuildInterface();

            ApplyFilters();
        }

        private void BuildInterface()
        {
            // Основная структура окна
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                ColumnCount = 1,
                RowCount = 2,
                BackColor = background
            };

            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 58));

            root.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            Controls.Add(root);

            // Верхняя панель
            var top = CreatePanel(Color.White, 12);

            top.Dock = DockStyle.Fill;
            top.Margin = new Padding(0, 0, 0, 12);

            root.Controls.Add(top, 0, 0);

            var brand = new Label
            {
                Text = "Адміністратор 1  ▼",
                Dock = DockStyle.Left,
                Width = 190,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Bold),
                ForeColor = accent
            };

            top.Controls.Add(brand);

            searchBox.PlaceholderText = "Пошук клієнта...";
            searchBox.Font = new Font("Segoe UI", 10F);
            searchBox.BorderStyle = BorderStyle.FixedSingle;
            searchBox.Size = new Size(230, 29);
            searchBox.Location = new Point(205, 14);
            searchBox.Anchor =
                AnchorStyles.Top | AnchorStyles.Left;

            top.Controls.Add(searchBox);

            searchBox.TextChanged += (s, e) => ApplyFilters();

            // Основная область: меню слева и содержимое справа
            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = background,
                Margin = new Padding(0)
            };

            body.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 220));

            body.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            root.Controls.Add(body, 0, 1);

            // ==========================================
            // ЛЕВАЯ БОКОВАЯ ПАНЕЛЬ
            // ==========================================

            var side = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 0, 12, 0),
                BackColor = background,
                Margin = new Padding(0)
            };

            side.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            side.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));

            body.Controls.Add(side, 0, 0);

            var navigation = CreatePanel(Color.White, 12);

            navigation.Dock = DockStyle.Fill;
            navigation.Padding = new Padding(8);

            side.Controls.Add(navigation, 0, 0);

            var navigationList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            navigation.Controls.Add(navigationList);

            string[] menuItems =
            {
                "Головна",
                "Покупці",
                "Ювелірні вироби",
                "Продажі та замовлення",
                "Постачання",
                "Ремонт виробів"
            };

            string[] menuIcons =
            {
                "▦",
                "👤",
                "◇",
                "⇄",
                "♧",
                "⚒"
            };

            for (int i = 0; i < menuItems.Length; i++)
            {
                bool selected = i == 1;

                var navigationButton = new Button
                {
                    Text = menuIcons[i] + "    " + menuItems[i],
                    Width = 180,
                    Height = 43,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 2, 0),

                    BackColor = selected
                        ? Color.FromArgb(232, 240, 255)
                        : Color.White,

                    ForeColor = selected
                        ? accent
                        : textColor,

                    Font = new Font(
                        "Segoe UI",
                        9F,
                        selected
                            ? FontStyle.Bold
                            : FontStyle.Regular),

                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 0, 2),
                    UseVisualStyleBackColor = false
                };

                navigationButton.FlatAppearance.BorderSize = 0;

                navigationList.Controls.Add(navigationButton);
            }

            // Нижняя часть боковой панели
            var sideFooter = CreatePanel(Color.White, 12);

            sideFooter.Dock = DockStyle.Fill;

            side.Controls.Add(sideFooter, 0, 1);

            var footerLabel = new Label
            {
                Text = "Активність робочого дня --- -",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8F)
            };

            sideFooter.Controls.Add(footerLabel);

            // ==========================================
            // ПРАВАЯ ЧАСТЬ ИНТЕРФЕЙСА
            // ==========================================

            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = background,
                Margin = new Padding(0)
            };

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 100));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 95));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 120));

            content.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 35));

            content.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            body.Controls.Add(content, 1, 0);

            // Карточки Find person / All people
            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = background,
                Margin = new Padding(0, 0, 0, 8)
            };

            content.Controls.Add(actions, 0, 0);

            actions.Controls.Add(
                CreateActionCard("⌕", "Find person", false));

            actions.Controls.Add(
                CreateActionCard("👤", "All people", true));

            // ==========================================
            // СТАТИСТИКА И КНОПКА PDF
            // ==========================================

            var stats = CreatePanel(Color.White, 12);

            stats.Dock = DockStyle.Fill;
            stats.Padding = new Padding(14);
            stats.Margin = new Padding(0, 0, 0, 10);

            content.Controls.Add(stats, 0, 1);

            var statsTitle = new Label
            {
                Text = "List of active people",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font(
                    "Segoe UI",
                    12F,
                    FontStyle.Bold),
                ForeColor = textColor
            };

            stats.Controls.Add(statsTitle);

            salesLabel.Text = "Кількість продажів: 0";
            salesLabel.Location = new Point(0, 38);
            salesLabel.Size = new Size(280, 32);
            salesLabel.TextAlign = ContentAlignment.MiddleLeft;

            salesLabel.Font = new Font(
                "Segoe UI",
                11F,
                FontStyle.Bold);

            salesLabel.ForeColor = accent;

            stats.Controls.Add(salesLabel);

            exportPdfButton.Text = "Завантажити звіт PDF";
            exportPdfButton.Size = new Size(190, 36);
            exportPdfButton.Location = new Point(0, 35);
            exportPdfButton.Anchor =
                AnchorStyles.Top | AnchorStyles.Right;

            exportPdfButton.FlatStyle = FlatStyle.Flat;
            exportPdfButton.FlatAppearance.BorderSize = 0;
            exportPdfButton.BackColor = accent;
            exportPdfButton.ForeColor = Color.White;

            exportPdfButton.Font = new Font(
                "Segoe UI",
                9F,
                FontStyle.Bold);

            exportPdfButton.Cursor = Cursors.Hand;
            exportPdfButton.Click += ExportPdf;

            stats.Controls.Add(exportPdfButton);

            stats.Resize += (s, e) =>
            {
                exportPdfButton.Location = new Point(
                    stats.ClientSize.Width -
                    exportPdfButton.Width - 14,
                    38);
            };

            // ==========================================
            // ФИЛЬТРЫ
            // ==========================================

            var filterPanel = CreatePanel(Color.White, 12);

            filterPanel.Dock = DockStyle.Fill;
            filterPanel.Padding = new Padding(12);
            filterPanel.Margin = new Padding(0, 0, 0, 10);

            content.Controls.Add(filterPanel, 0, 2);

            var filterTitle = new Label
            {
                Text = "Фільтри звіту",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold),
                ForeColor = textColor
            };

            filterPanel.Controls.Add(filterTitle);

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(0, 8, 0, 0),
                BackColor = Color.White
            };

            filterPanel.Controls.Add(filterFlow);
            filterFlow.BringToFront();

            // Фильтр по городу
            AddFilter(
                filterFlow,
                "Місто:",
                cityFilter,
                new[]
                {
                    "Усі міста",
                    "Київ",
                    "Харків"
                });

            // Фильтр по отделению
            AddFilter(
                filterFlow,
                "Відділення:",
                branchFilter,
                new[]
                {
                    "Усі відділення",
                    "Київ (3-є відділення)",
                    "Харків (1-ше відділення)",
                    "Харків (2-ге відділення)"
                });

            // Фильтр по отделу
            AddFilter(
                filterFlow,
                "Відділ:",
                divisionFilter,
                new[]
                {
                    "Усі відділи",
                    "Management",
                    "Finance",
                    "IT",
                    "Marketing"
                });

            cityFilter.SelectedIndexChanged += (s, e) =>
            {
                UpdateBranchOptions();
                ApplyFilters();
            };

            branchFilter.SelectedIndexChanged += (s, e) =>
                ApplyFilters();

            divisionFilter.SelectedIndexChanged += (s, e) =>
                ApplyFilters();

            // Кнопка очистки фильтров
            var clearButton = new Button
            {
                Text = "Очистити фільтри",
                Size = new Size(145, 32),
                Margin = new Padding(12, 22, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = textColor,
                Cursor = Cursors.Hand
            };

            clearButton.Click += (s, e) =>
            {
                searchBox.Clear();

                cityFilter.SelectedIndex = 0;
                divisionFilter.SelectedIndex = 0;

                UpdateBranchOptions();

                branchFilter.SelectedIndex = 0;

                ApplyFilters();
            };

            filterFlow.Controls.Add(clearButton);

            // ==========================================
            // ЗАГОЛОВОК ТАБЛИЦЫ
            // ==========================================

            var tableHeader = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = background,
                Margin = new Padding(0)
            };

            content.Controls.Add(tableHeader, 0, 3);

            countLabel.Text = "Клієнтів у звіті: 0";
            countLabel.Dock = DockStyle.Left;
            countLabel.Width = 260;
            countLabel.TextAlign = ContentAlignment.MiddleLeft;

            countLabel.Font = new Font("Segoe UI", 10F);
            countLabel.ForeColor = textColor;

            tableHeader.Controls.Add(countLabel);

            // ==========================================
            // ТАБЛИЦА КЛИЕНТОВ
            // ==========================================

            var tablePanel = CreatePanel(Color.White, 12);

            tablePanel.Dock = DockStyle.Fill;
            tablePanel.Padding = new Padding(8);

            content.Controls.Add(tablePanel, 0, 4);

            ConfigureGrid();

            peopleGrid.Dock = DockStyle.Fill;

            tablePanel.Controls.Add(peopleGrid);

            // Инициализация фильтров
            cityFilter.SelectedIndex = 0;
            divisionFilter.SelectedIndex = 0;

            UpdateBranchOptions();

            branchFilter.SelectedIndex = 0;
        }

        // Создание элемента фильтра
        private void AddFilter(
            FlowLayoutPanel parent,
            string caption,
            ComboBox combo,
            string[] options)
        {
            var container = new Panel
            {
                Width = caption == "Відділення:" ? 330 : 220,
                Height = 65,
                Margin = new Padding(0, 0, 10, 0),
                BackColor = Color.White
            };

            var label = new Label
            {
                Text = caption,
                Dock = DockStyle.Top,
                Height = 23,
                ForeColor = textColor,
                Font = new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold)
            };

            container.Controls.Add(label);

            combo.DropDownStyle =
                ComboBoxStyle.DropDownList;

            combo.Font = new Font("Segoe UI", 9F);
            combo.Width = container.Width - 5;
            combo.Location = new Point(0, 27);

            combo.Items.AddRange(options);

            container.Controls.Add(combo);

            parent.Controls.Add(container);
        }

        // Обновление списка отделений при смене города
        private void UpdateBranchOptions()
        {
            string previous =
                Convert.ToString(branchFilter.SelectedItem)
                ?? "Усі відділення";

            string city =
                Convert.ToString(cityFilter.SelectedItem)
                ?? "Усі міста";

            branchFilter.Items.Clear();

            branchFilter.Items.Add("Усі відділення");

            if (city == "Усі міста" || city == "Київ")
            {
                branchFilter.Items.Add(
                    "Київ (3-є відділення)");
            }

            if (city == "Усі міста" || city == "Харків")
            {
                branchFilter.Items.Add(
                    "Харків (1-ше відділення)");

                branchFilter.Items.Add(
                    "Харків (2-ге відділення)");
            }

            int index =
                branchFilter.Items.IndexOf(previous);

            branchFilter.SelectedIndex =
                index >= 0 ? index : 0;
        }

        // Настройка таблицы
        private void ConfigureGrid()
        {
            peopleGrid.AllowUserToAddRows = false;
            peopleGrid.AllowUserToDeleteRows = false;
            peopleGrid.AllowUserToResizeRows = false;
            peopleGrid.ReadOnly = true;
            peopleGrid.RowHeadersVisible = false;
            peopleGrid.MultiSelect = false;

            peopleGrid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            peopleGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            peopleGrid.BackgroundColor = Color.White;
            peopleGrid.BorderStyle = BorderStyle.None;

            peopleGrid.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            peopleGrid.GridColor =
                Color.FromArgb(235, 237, 241);

            peopleGrid.EnableHeadersVisualStyles = false;
            peopleGrid.ColumnHeadersHeight = 38;
            peopleGrid.RowTemplate.Height = 35;

            peopleGrid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(235, 238, 243);

            peopleGrid.ColumnHeadersDefaultCellStyle.ForeColor =
                textColor;

            peopleGrid.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    9F,
                    FontStyle.Bold);

            peopleGrid.DefaultCellStyle.Font =
                new Font("Segoe UI", 9F);

            peopleGrid.DefaultCellStyle.ForeColor =
                textColor;

            peopleGrid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(225, 237, 255);

            peopleGrid.DefaultCellStyle.SelectionForeColor =
                textColor;

            peopleGrid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(249, 250, 252);

            peopleGrid.Columns.Add(
                "FullName",
                "Клієнт");

            peopleGrid.Columns.Add(
                "City",
                "Місто");

            peopleGrid.Columns.Add(
                "Branch",
                "Відділення");

            peopleGrid.Columns.Add(
                "Division",
                "Відділ");

            peopleGrid.Columns.Add(
                "SalesCount",
                "Кількість продажів");

            peopleGrid.Columns.Add(
                "ClientId",
                "ID клієнта");
        }

        // Демонстрационные данные.
        // Замените их данными из своей базы данных для реальной отчётности.
        private void AddSampleClients()
        {
            clients.AddRange(new[]
            {
                new ClientRecord(
                    "Олена Коваль",
                    "Київ",
                    "Київ (3-є відділення)",
                    "Management",
                    3,
                    "1001"),

                new ClientRecord(
                    "Андрій Мельник",
                    "Київ",
                    "Київ (3-є відділення)",
                    "Finance",
                    2,
                    "1002"),

                new ClientRecord(
                    "Марія Шевченко",
                    "Харків",
                    "Харків (1-ше відділення)",
                    "IT",
                    5,
                    "1003"),

                new ClientRecord(
                    "Іван Бондар",
                    "Харків",
                    "Харків (2-ге відділення)",
                    "Marketing",
                    1,
                    "1004"),

                new ClientRecord(
                    "Софія Ткаченко",
                    "Харків",
                    "Харків (1-ше відділення)",
                    "Finance",
                    4,
                    "1005"),

                new ClientRecord(
                    "Дмитро Кравець",
                    "Київ",
                    "Київ (3-є відділення)",
                    "IT",
                    6,
                    "1006"),

                new ClientRecord(
                    "Анна Мороз",
                    "Харків",
                    "Харків (2-ге відділення)",
                    "Management",
                    2,
                    "1007")
            });
        }

        // Получение клиентов с учётом всех фильтров
        private List<ClientRecord> GetFilteredClients()
        {
            string query = searchBox.Text.Trim();

            string city =
                Convert.ToString(cityFilter.SelectedItem)
                ?? "Усі міста";

            string branch =
                Convert.ToString(branchFilter.SelectedItem)
                ?? "Усі відділення";

            string division =
                Convert.ToString(divisionFilter.SelectedItem)
                ?? "Усі відділи";

            return clients.Where(c =>
                (city == "Усі міста" ||
                 c.City == city) &&

                (branch == "Усі відділення" ||
                 c.Branch == branch) &&

                (division == "Усі відділи" ||
                 c.Division == division) &&

                (query.Length == 0 ||
                 c.FullName.Contains(
                     query,
                     StringComparison.OrdinalIgnoreCase) ||

                 c.ClientId.Contains(
                     query,
                     StringComparison.OrdinalIgnoreCase) ||

                 c.City.Contains(
                     query,
                     StringComparison.OrdinalIgnoreCase) ||

                 c.Branch.Contains(
                     query,
                     StringComparison.OrdinalIgnoreCase) ||

                 c.Division.Contains(
                     query,
                     StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        // Обновление таблицы и статистики
        private void ApplyFilters()
        {
            if (peopleGrid.Columns.Count == 0)
                return;

            var filtered = GetFilteredClients();

            peopleGrid.Rows.Clear();

            foreach (var client in filtered)
            {
                peopleGrid.Rows.Add(
                    client.FullName,
                    client.City,
                    client.Branch,
                    client.Division,
                    client.SalesCount,
                    client.ClientId);
            }

            countLabel.Text =
                $"Клієнтів у звіті: {filtered.Count}";

            salesLabel.Text =
                $"Кількість продажів: {filtered.Sum(c => c.SalesCount)}";
        }

        // Экспорт PDF-отчёта по выбранным фильтрам
        private void ExportPdf(object? sender, EventArgs e)
        {
            var filtered = GetFilteredClients();

            using var dialog = new SaveFileDialog
            {
                Title = "Зберегти звіт про продажі",

                Filter = "PDF files (*.pdf)|*.pdf",

                FileName =
                    $"zvit_prodazhi_{DateTime.Now:yyyy-MM-dd}.pdf",

                DefaultExt = "pdf",
                AddExtension = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string city =
                Convert.ToString(cityFilter.SelectedItem)
                ?? "Усі міста";

            string branch =
                Convert.ToString(branchFilter.SelectedItem)
                ?? "Усі відділення";

            string division =
                Convert.ToString(divisionFilter.SelectedItem)
                ?? "Усі відділи";

            try
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(28);

                        page.DefaultTextStyle(
                            x => x.FontSize(9));

                        // Заголовок отчёта
                        page.Header().Column(column =>
                        {
                            column.Item()
                                .Text("JEWELRYSTORE — ЗВІТ ПРО ПРОДАЖІ")
                                .FontSize(18)
                                .Bold()
                                .FontColor("#3070E1");

                            column.Item().Text(
                                $"Дата формування: {DateTime.Now:dd.MM.yyyy HH:mm}");

                            column.Item().Text(
                                $"Місто: {city}   |   Відділення: {branch}   |   Відділ: {division}");

                            column.Item()
                                .Text(
                                    $"Клієнтів у звіті: {filtered.Count}   |   Кількість продажів: {filtered.Sum(c => c.SalesCount)}")
                                .Bold();
                        });

                        // Таблица отчёта
                        page.Content()
                            .PaddingVertical(15)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2.4f);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(2.2f);
                                    columns.RelativeColumn(1.4f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1f);
                                });

                                table.Header(header =>
                                {
                                    HeaderCell(header, "Клієнт");
                                    HeaderCell(header, "Місто");
                                    HeaderCell(header, "Відділення");
                                    HeaderCell(header, "Відділ");
                                    HeaderCell(header, "К-сть продажів");
                                    HeaderCell(header, "ID клієнта");
                                });

                                foreach (var client in filtered)
                                {
                                    BodyCell(
                                        table,
                                        client.FullName);

                                    BodyCell(
                                        table,
                                        client.City);

                                    BodyCell(
                                        table,
                                        client.Branch);

                                    BodyCell(
                                        table,
                                        client.Division);

                                    BodyCell(
                                        table,
                                        client.SalesCount.ToString());

                                    BodyCell(
                                        table,
                                        client.ClientId);
                                }

                                table.Footer(footer =>
                                {
                                    footer.Cell()
                                        .ColumnSpan(4)
                                        .Background("#E9EFFA")
                                        .Padding(6)
                                        .Text("УСЬОГО")
                                        .Bold();

                                    footer.Cell()
                                        .Background("#E9EFFA")
                                        .Padding(6)
                                        .Text(
                                            filtered.Sum(c => c.SalesCount).ToString())
                                        .Bold();

                                    footer.Cell()
                                        .Background("#E9EFFA")
                                        .Padding(6)
                                        .Text(
                                            $"Клієнтів: {filtered.Count}")
                                        .Bold();
                                });
                            });

                        // Нумерация страниц
                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span(
                                    "JewelryStore  •  Сторінка ");

                                text.CurrentPageNumber();

                                text.Span(" з ");

                                text.TotalPages();
                            });
                    });
                }).GeneratePdf(dialog.FileName);

                MessageBox.Show(
                    "PDF-звіт успішно збережено.",
                    "JewelryStore",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не вдалося створити PDF-звіт:\n" + ex.Message,
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Ячейка заголовка PDF
        private static void HeaderCell(
            TableCellDescriptor header,
            string value)
        {
            header.Cell()
                .Background("#3070E1")
                .Padding(6)
                .Text(value)
                .FontColor(Colors.White)
                .Bold();
        }

        // Ячейка данных PDF
        private static void BodyCell(
            TableDescriptor table,
            string value)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor("#E6E9EF")
                .Padding(6)
                .Text(value ?? "");
        }

        // Карточки Find person / All people
        private Panel CreateActionCard(
            string symbol,
            string caption,
            bool selected)
        {
            var card = CreatePanel(
                selected ? accent : Color.White,
                12);

            card.Size = new Size(175, 94);

            card.Margin = new Padding(0, 0, 12, 0);

            var icon = new Label
            {
                Text = symbol,
                Dock = DockStyle.Top,
                Height = 51,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Symbol", 22F),

                ForeColor = selected
                    ? Color.White
                    : textColor
            };

            var title = new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F),

                ForeColor = selected
                    ? Color.White
                    : textColor
            };

            card.Controls.Add(title);
            card.Controls.Add(icon);

            return card;
        }

        // Создание панели со скруглёнными углами
        private Panel CreatePanel(
            Color color,
            int radius)
        {
            var panel = new Panel
            {
                BackColor = color,
                Margin = new Padding(0)
            };

            panel.Resize += (s, e) =>
            {
                if (panel.Width > 0 && panel.Height > 0)
                {
                    panel.Region = CreateRoundedRegion(
                        panel.Width,
                        panel.Height,
                        radius);
                }
            };

            return panel;
        }

        private Region CreateRoundedRegion(
            int width,
            int height,
            int radius)
        {
            var path =
                new System.Drawing.Drawing2D.GraphicsPath();

            int diameter = Math.Min(
                radius * 2,
                Math.Min(width, height));

            path.AddArc(
                0,
                0,
                diameter,
                diameter,
                180,
                90);

            path.AddArc(
                width - diameter,
                0,
                diameter,
                diameter,
                270,
                90);

            path.AddArc(
                width - diameter,
                height - diameter,
                diameter,
                diameter,
                0,
                90);

            path.AddArc(
                0,
                height - diameter,
                diameter,
                diameter,
                90,
                90);

            path.CloseFigure();

            return new Region(path);
        }

        // Модель клиента
        private sealed class ClientRecord
        {
            public string FullName { get; }

            public string City { get; }

            public string Branch { get; }

            public string Division { get; }

            public int SalesCount { get; }

            public string ClientId { get; }

            public ClientRecord(
                string fullName,
                string city,
                string branch,
                string division,
                int salesCount,
                string clientId)
            {
                FullName = fullName;
                City = city;
                Branch = branch;
                Division = division;
                SalesCount = salesCount;
                ClientId = clientId;
            }
        }
    }
}