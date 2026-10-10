# Blazify

**A modern, themeable, RTL-first Blazor UI component library for .NET 8 and .NET 10.**

50+ pure CSS/SVG components — data grid, inputs, date & time pickers, Jalali (Persian) calendar, dialogs, charts, navigation, forms and more — with first-class right-to-left support, fa/ar/en localization and Tailwind-based theming. Works on Blazor Server, WebAssembly and the unified Blazor Web App model.

Website: **[blazify.ir](https://blazify.ir)**

> Free to use in personal and commercial projects. No account, no license key. Docs and live demos live at [blazify.ir](https://blazify.ir).

**Read this in:** English (below) · [فارسی](#blazify-fa)

---

## Table of contents

- [Why Blazify](#why-blazify)
- [Requirements](#requirements)
- [Getting started (step by step)](#getting-started)
- [Components](#components)
  - [Buttons & inputs](#buttons--inputs)
  - [Forms & validation](#forms--validation)
  - [Selection controls](#selection-controls)
  - [Data (Grid & Chart)](#data)
  - [Date & time](#date--time)
  - [Navigation](#navigation)
  - [Overlays](#overlays)
  - [Feedback](#feedback)
  - [Display](#display)
  - [Iranian toolkit](#iranian-toolkit)
- [Theming & customization](#theming)
- [Localization](#localization)
- [License](#license)

---

## Why Blazify

- **RTL-first.** Every component is built for RTL and LTR from day one using CSS logical properties. Flip direction at runtime with one call.
- **50+ components.** Grid, inputs, date/time pickers, a full Jalali calendar, dialogs, SVG charts, menus, wizards, forms and a lot more.
- **Lightweight.** The only hard dependency is `Microsoft.AspNetCore.Components.Web`. Styling is plain CSS, icons are plain SVG.
- **Themeable.** Tailwind-based color system with Light, Dark, Light Gray and Light Purple themes, plus Modern / ... 'Coming Soon' appearances. Everything is driven by CSS variables you can override.
- **Localized.** Built-in Persian (fa), Arabic (ar) and English (en) with automatic direction switching.
- **Iran-ready.** Built-in validation for national code, mobile, IBAN (Sheba) and bank card, a Jalali calendar, Persian digit handling and number-to-words.

## Requirements

- .NET 8.0 or .NET 10.0
- Blazor Server, Blazor WebAssembly, or Blazor Web App
- A host page you can add `<link>` and `<script>` tags to (`App.razor`, `index.html`, or `_Host.cshtml`)

## Getting started

### 1. Install the package

```bash
dotnet add package Blazify
```

Or from the Package Manager Console:

```powershell
Install-Package Blazify
```

### 2. Register the services

In `Program.cs`, call `AddBlazify()`. You can pass options or leave it empty for defaults.

```csharp
using BzCore.Extensions;
using BzCore.Enums;
using BzCore.Theming;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazify(options =>
{
    options.Appearance   = BzAppearance.Modern;  // Modern
    options.DefaultTheme = BzThemeKind.Light;    // Light | Dark | LightGray | LightPurple
    options.Direction    = BzDirection.Rtl;      // Rtl | Ltr
    options.Font         = BzFont.IranSans;      // IranSans
    options.Dense        = true;                  // compact spacing across Blazify
});
```

Defaults are fine too:

```csharp
builder.Services.AddBlazify();
```

This registers `IBzThemeService`, `IBzDialogService`, `IBzNotificationService` and `IBzLocalizer`.

### 3. Add the styles and scripts to your host page

Blazify ships its assets under `_content/Blazify`. Add the single stylesheet and the single script to your host page (`App.razor`, `index.html` or `_Host.cshtml`). Use the exact lowercase paths below — they are case-sensitive on Linux hosting.

In `<head>`:

```html
<link rel="stylesheet" href="/_content/Blazify/css/blazify.css?v=1.2.0" />
```

Before the closing `</body>` (after `blazor.*.js`):

```html
<script src="/_content/Blazify/js/blazify.js?v=1.2.0"></script>
```

Font Awesome and Blazify fonts are included in the stylesheet. Versioned URLs let the browser cache the bundles; ASP.NET Core hosts can also set a long-lived `Cache-Control` header for `/_content/Blazify`. Optional third-party integrations such as CKEditor still require their vendor script.

### 4. Add the namespaces

In `_Imports.razor`, add the namespaces you use. `BzCore` is the root namespace (not `Blazify.Core`).

```razor
@using BzCore
@using BzCore.Enums
@using BzCore.Theming
@using BzCore.Helpers
@using BzCore.Localization
@using BzCore.Components.Button
@using BzCore.Components.Input
@using BzCore.Components.Form
@using BzCore.Components.Grid
@using BzCore.Components.DatePicker
@using BzCore.Components.Dialog
@using BzCore.Components.Notification
@using BzCore.Components.Navigation
@using BzCore.Components.Display
@using BzCore.Components.Overlay
@* ...add the rest as you need them *@
```

### 5. Wrap your layout and add the hosts

`BzThemeProvider` sets the theme scope, direction and CSS variables. Put it around your layout, and drop `BzDialogHost` and `BzNotificationHost` inside it once (so dialogs and toasts have somewhere to render).

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<BzThemeProvider>
    <main>
        @Body
    </main>

    <BzDialogHost />
    <BzNotificationHost />
</BzThemeProvider>
```

That's it — you're ready to use components.

### 6. Optional dependencies (only for a few components)

Most components need nothing extra. A handful use popular third-party libraries you load from a CDN in your host page:

| Component | Load this in your host page |
| --- | --- |
| Icons (used across the library via `fa-solid ...` classes) | [Font Awesome](https://fontawesome.com) |
| `BzEditor` (rich text) | CKEditor 5 (Classic build) |
| `BzCarousel` | Swiper (JS + CSS) |

If a library isn't loaded, that one component degrades gracefully and logs a hint in the console — the rest of Blazify keeps working.

## Components

Every example below is complete and runnable once you've done the setup above. Component parameters that aren't shown fall back to sensible defaults.

### Buttons & inputs

#### BzButton

```razor
<BzButton Text="Save"
          Color="BzColor.Primary"
          Variant="BzButtonVariant.Filled"
          Size="BzSize.Medium"
          StartIcon="fa-solid fa-floppy-disk"
          EndIcon="fa-solid fa-arrow-right"
          Loading="isSaving"
          AutoLoadingOnClick="true"
          LoadingEffect="BzLoadingEffect.Spinner"
          Ripple="true"
          Rounded="false"
          FullWidth="false"
          Disabled="false"
          ButtonType="button"
          OnClick="OnSave" />

@code {
    bool isSaving;
    async Task OnSave() { isSaving = true; await Task.Delay(800); isSaving = false; }
}
```

`Color`: Default, Primary, Secondary, Success, Info, Warning, Danger, Surface · `Variant`: Filled, Outlined, Text, Soft · `Size`: Small, Medium, Large.

#### BzInput

A single text input that covers plain text, money and masked modes, character filtering and Persian-digit handling. It's generic over the bound type (`TValue`).

```razor
@* Plain text with clear button and icons *@
<BzInput TValue="string" @bind-Value="name"
         Label="Full name" Placeholder="Type your name"
         StartIcon="fa-solid fa-user" Clearable Required
         CharFilter="BzCharFilter.PersianAndEnglish"
         MaxLength="60" HelperText="As printed on your ID" />

@* Money input that also renders the amount in words *@
<BzInput TValue="decimal?" @bind-Value="price"
         Label="Price" Mode="BzInputMode.Money"
         Currency="BzCurrency.Toman"
         ShowAmountInWords="true" AmountInWordsSuffix="Toman" />

@* Masked input (e.g. a 10-digit code) *@
<BzInput TValue="string" @bind-Value="code"
         Label="Code" Mode="BzInputMode.Mask" Mask="##########"
         ShowPersianDigits="false" UpdateTrigger="BzUpdateTrigger.OnInput" />

@code {
    string? name; decimal? price; string? code;
}
```

`Mode`: Normal, Money, Mask · `CharFilter`: Any, PersianOnly, EnglishOnly, PersianAndEnglish, NumberOnly, Decimal · `Currency`: Rial, Toman, Dollar, Pound, Euro.

#### BzInputGroup

A text field with prepended/appended content and several visual variants.

```razor
<BzInputGroup @bind-Value="website"
              Label="Website" Placeholder="yoursite"
              Hint="Add your domain" Variant="BzInputGroupVariant.Outlined">
    <Prepend>https://</Prepend>
    <Append>.com</Append>
</BzInputGroup>

@code { string? website; }
```

`Variant`: Default, Filled, Outlined, Pill, Underline, Seamless, Rounded.

#### BzAddonGroup

Wrap any inputs/buttons together with shared prepend/append add-ons.

```razor
<BzAddonGroup Variant="BzInputGroupVariant.Filled">
    <Prepend><i class="fa-solid fa-magnifying-glass"></i></Prepend>
    <ChildContent>
        <BzInput TValue="string" @bind-Value="term" Placeholder="Search..." />
    </ChildContent>
    <Append><BzButton Text="Go" Size="BzSize.Small" /></Append>
</BzAddonGroup>

@code { string? term; }
```

#### BzButtonGroup

A set of connected buttons that can act as a single- or multi-select toggle.

```razor
<BzButtonGroup TValue="string" @bind-Value="view"
               Variant="BzButtonGroupVariant.Toggle"
               Orientation="BzOrientation.Horizontal"
               Size="BzSize.Medium"
               Items="viewItems" />

@code {
    string? view = "grid";
    List<BzButtonGroupItem<string>> viewItems = new()
    {
        new("grid", "Grid", "fa-solid fa-table-cells"),
        new("list", "List", "fa-solid fa-list"),
        new("board","Board","fa-solid fa-columns"),
    };
}
```

`Variant`: Attached, Separated, Pills, Outlined, Toggle. Use `@bind-Values` + `Multiple` for multi-select.

### Selection controls

#### BzAutoComplete

Typeahead over a static list or an async search function.

```razor
@* Static list *@
<BzAutoComplete TItem="string" @bind-Value="city"
                Label="City" Placeholder="Search city..."
                Items="cities" Clearable
                MinSearchLength="1" HighlightMatch="true" OpenOnFocus="true" />

@* Async search *@
<BzAutoComplete TItem="string" @bind-Value="asyncCity"
                Label="City (async)" SearchFunc="SearchCitiesAsync"
                MinSearchLength="1" DebounceMs="250" MaxItems="20"
                NoResultsText="Nothing found" />

@code {
    string? city, asyncCity;
    string[] cities = { "Tehran", "Shiraz", "esfahan", "Tabriz" };
    Task<IEnumerable<string>> SearchCitiesAsync(string q)
        => Task.FromResult(cities.Where(c => c.Contains(q, StringComparison.OrdinalIgnoreCase)));
}
```

#### BzSelect

```razor
<BzSelect TValue="int" @bind-Value="cityId"
          Label="City" Placeholder="Choose..."
          Items="options" Clearable />

@code {
    int cityId;
    List<BzOption<int>> options = new()
    {
        new(1, "Tehran", "fa-solid fa-city"),
        new(2, "Shiraz"),
        new(3, "esfahan"),
    };
}
```


#### BzCheckbox

```razor
<BzCheckbox @bind-Value="agree" Label="I accept the terms"
            Color="BzColor.Primary" Shape="BzCheckShape.Rounded" Card="false" />

@code { bool agree; }
```

`Shape`: Square, Rounded, Circle.

#### BzSwitch

```razor
<BzSwitch @bind-Value="enabled" Label="Notifications"
          Size="BzSize.Medium" Color="BzColor.Success" ShowIcons="true" />

@code { bool enabled = true; }
```

#### BzRadioGroup

```razor
<BzRadioGroup TValue="string" @bind-Value="plan"
              Items="plans" Orientation="BzOrientation.Vertical"
              Color="BzColor.Primary" Card="true" />

@code {
    string? plan = "pro";
    List<BzOption<string>> plans = new() { new("free","Free"), new("pro","Pro"), new("team","Team") };
}
```

#### BzSlider

```razor
<BzSlider @bind-Value="volume" Label="Volume"
          Min="0" Max="100" Step="5" ShowValue="true" />

@code { double volume = 40; }
```

#### BzRating

```razor
<BzRating @bind-Value="stars" Max="5" Icon="fa-solid fa-star"
          ShowValue="true" ReadOnly="false" />

@code { int stars = 3; }
```

#### BzNumberStepper

```razor
<BzNumberStepper @bind-Value="qty" Label="Quantity"
                 Min="1" Max="99" StepValue="1" />

@code { double qty = 1; }
```

#### BzColorPicker

```razor
<BzColorPicker @bind-Value="color" Label="Brand color" Swatches="swatches" />

@code {
    string? color = "#2563eb";
    string[] swatches = { "#2563eb", "#16a34a", "#dc2626", "#9333ea" };
}
```

#### BzTimePicker

```razor
<BzTimePicker @bind-Value="time" Label="Meeting time" Seconds="false" />

@code { string? time = "09:30"; }
```

#### BzTagInput

```razor
<BzTagInput @bind-Values="keywords" Label="Keywords" Placeholder="Type and press Enter" />

@code { List<string> keywords = new() { "blazor", "rtl" }; }
```

#### BzOtpInput

```razor
<BzOtpInput @bind-Value="otp" Length="6" />

@code { string? otp; }
```

#### BzFieldset

Group related fields with a title, icon and optional collapse.

```razor
<BzFieldset Title="Contact info" Icon="fa-solid fa-address-book"
            Variant="BzFieldsetVariant.Card" Color="BzColor.Primary"
            Collapsible="true" @bind-Collapsed="collapsed">
    <BzInput TValue="string" @bind-Value="email" Label="Email" />
    <BzInput TValue="string" @bind-Value="phone" Label="Phone" />
</BzFieldset>

@code { bool collapsed; string? email; string? phone; }
```

`Variant`: Legend, Card, Filled.

#### BzBankInput (Iranian identifiers)

One field for Iranian identifiers with grouping, live validation and automatic bank detection.

```razor
@* Bank card: 16 digits, grouped, detects the bank *@
<BzBankInput Type="BzIdentifierType.CardNumber" @bind-Value="card"
             Label="Card number" ShowBank="true" ShowValidation="true"
             OnValidated="OnCardValidated" />

@* Sheba / IBAN: IR + 24 digits with mod-97 check and bank detection *@
<BzBankInput Type="BzIdentifierType.Iban" @bind-Value="iban"
             Label="Sheba (IBAN)" ShowBank="true" ShowValidation="true" />

@* National code: 10 digits *@
<BzBankInput Type="BzIdentifierType.NationalCode" @bind-Value="nationalCode"
             Label="National code" ShowValidation="true" />

@code {
    string? card, iban, nationalCode;
    void OnCardValidated(IdentifierValidationResult r) { /* r.IsValid, detected bank, etc. */ }
}
```

`Type`: CardNumber, Iban, AccountNumber, NationalCode, LegalId, ForeignerId, Passport.

### Data

#### BzGrid

`BzGrid` supports sorting, filtering, paging, server data providers, inline editing,
details, grouping, export, and single or arbitrary multi-row selection. A dynamic
predicate can fully lock financial/finalized rows:

```razor
<BzGrid TItem="Order" Items="orders"
        Selectable MultiSelect
        @bind-SelectedItems="selected"
        RowDisabled="@(order => order.IsFinalized)"
        HeaderActions="actions">
    <Columns>
        <BzColumn TItem="Order" Title="Id" Field="@(order => order.Id)" />
        <BzColumn TItem="Order" Title="Amount" Field="@(order => order.Amount)" />
    </Columns>
</BzGrid>

@code {
    IReadOnlyCollection<Order> selected = [];

    IReadOnlyList<BzGridHeaderAction<Order>> actions =
    [
        new() { Title = "Create", SelectionRequirement = BzGridActionSelectionRequirement.None },
        new() { Title = "View", SelectionRequirement = BzGridActionSelectionRequirement.ExactlyOne },
        new() { Title = "Approve", SelectionRequirement = BzGridActionSelectionRequirement.OneOrMore }
    ];
}
```

Disabled rows cannot be selected (including Select All), clicked, expanded, edited,
or operated through controls inside their templates. Header actions are disabled until
their selection requirement is satisfied.

### Date & time

#### BzDatePicker

Jalali (Persian) or Gregorian, single date or range, with optional Persian digits and an open-ended range.

```razor
@* Single Jalali date *@
<BzDatePicker @bind-Value="date"
              Calendar="BzCalendarType.Jalali" PersianDigits="true"
              Min="DateTime.Today.AddYears(-1)" Max="DateTime.Today.AddYears(1)" />

@* Range with an optional "until further notice" end *@
<BzDatePicker @bind-Value="from" @bind-EndValue="to"
              Range="true" AllowUntilFurtherNotice="true"
              Calendar="BzCalendarType.Jalali" />

@code { DateTime? date; DateTime? from; DateTime? to; }
```

`Calendar`: Jalali, Gregorian.

#### BzCalendarView

An inline month view with events.

```razor
<BzCalendarView @bind-SelectedDate="day"
                Calendar="BzCalendarType.Jalali" PersianDigits="true"
                ShowHolidays="true" Events="events"
                OnDayClick="d => day = d" />

@code {
    DateTime? day = DateTime.Today;
    List<BzCalendarEvent> events = new()
    {
        new(DateTime.Today, "Team sync", BzColor.Primary),
        new(DateTime.Today.AddDays(2), "Release", BzColor.Success),
    };
}
```

#### BzCalendarFull

A full-page calendar with multiple events per day.

```razor
<BzCalendarFull @bind-SelectedDate="day"
                Calendar="BzCalendarType.Jalali" PersianDigits="true"
                ShowHolidays="true" MaxEventsPerDay="3" Events="events" />

@code {
    DateTime? day = DateTime.Today;
    List<BzCalendarEvent> events = new() { new(DateTime.Today, "Standup", BzColor.Info) };
}
```

### Navigation

#### BzNavbar

```razor
<BzNavbar Variant="BzNavbarVariant.Glass" Sticky="true">
    <Brand><i class="fa-solid fa-cubes"></i> My App</Brand>
    <Center>
        <BzMenu Items="menu" Orientation="BzOrientation.Horizontal" />
    </Center>
    <End>
        <BzButton Text="Sign in" Size="BzSize.Small" Variant="BzButtonVariant.Soft" />
    </End>
</BzNavbar>

@code {
    List<BzMenuItem> menu = new()
    {
        new() { Text = "Home", Href = "/" },
        new() { Text = "Docs", Href = "/docs" },
    };
}
```

`Variant`: Solid, Light, Dark, Gradient, Glass, Bordered, Elevated, Minimal, Accent, Centered.

#### BzMenu

Supports nested children, icons, badges and click actions.

```razor
<BzMenu Items="items" Variant="BzMenuVariant.Pills" Orientation="BzOrientation.Horizontal" />

@code {
    List<BzMenuItem> items = new()
    {
        new() { Text = "Dashboard", Icon = "fa-solid fa-gauge", Href = "/" },
        new() { Text = "Products",  Icon = "fa-solid fa-box", Badge = "New",
                Children = new() {
                    new() { Text = "All", Href = "/products" },
                    new() { Text = "Add", Href = "/products/new" },
                }},
        new() { Text = "Logout", Icon = "fa-solid fa-right-from-bracket",
                OnClick = () => Console.WriteLine("bye") },
    };
}
```

`Variant`: Underline, Pills, Solid, Soft, Bordered, Minimal, Boxed, Accent, Gradient, Dark.

#### BzBreadcrumbs

```razor
<BzBreadcrumbs Items="crumbs" Separator="BzBreadcrumbSeparator.Chevron" />

@code {
    List<BzBreadcrumbItem> crumbs = new()
    {
        new() { Text = "Home", Icon = "fa-solid fa-house", Href = "/" },
        new() { Text = "Products", Href = "/products" },
        new() { Text = "Details" },
    };
}
```

`Separator`: Chevron, Slash, Arrow, Dot.

#### BzSidebar

A collapsible side panel. Compose it with `BzNavGroup` and `BzNavItem`.

```razor
<BzSidebar @bind-Collapsed="collapsed" Width="260px" CollapsedWidth="72px"
           Variant="BzSidebarVariant.Floating" ShowToggle="true">
    <HeaderContent><b>My App</b></HeaderContent>
    <ChildContent>
        <BzNavGroup Title="Main">
            <BzNavItem Text="Dashboard" Icon="fa-solid fa-gauge" Href="/" />
            <BzNavItem Text="Orders" Icon="fa-solid fa-cart-shopping" Href="/orders" BadgeText="8" />
        </BzNavGroup>
        <BzNavGroup Title="Settings">
            <BzNavItem Text="Profile" Icon="fa-solid fa-user" Href="/profile" />
        </BzNavGroup>
    </ChildContent>
    <FooterContent>v1.0.0</FooterContent>
</BzSidebar>

@code { bool collapsed; }
```

`Variant`: Default, Dark, Primary, Glass, Floating.

#### BzFloatingBox

A floating action box anchored to a corner.

```razor
<BzFloatingBox Icon="fa-solid fa-headset" Title="Support"
               Position="BzFloatingPosition.BottomEnd" @bind-Open="open">
    <p>How can we help?</p>
    <BzButton Text="Start chat" Size="BzSize.Small" />
</BzFloatingBox>

@code { bool open; }
```

`Position`: BottomEnd, BottomStart, TopEnd, TopStart.

#### BzTreeView

Generic tree with selection, search and per-node actions.

```razor
<BzTreeView TItem="Node" Items="roots"
            ChildrenSelector="n => n.Children"
            TextSelector="n => n.Name"
            IconSelector="n => n.Icon"
            Selectable MultiSelect Searchable
            ExpandedByDefault="false"
            OnNodeClick="n => Console.WriteLine(n.Name)" />

@code {
    List<Node> roots = new()
    {
        new("src", "fa-solid fa-folder", new()
        {
            new("Program.cs", "fa-solid fa-file-code"),
            new("wwwroot", "fa-solid fa-folder"),
        }),
    };
    public record Node(string Name, string? Icon = null, List<Node>? Children = null);
}
```

#### BzPagination

```razor
<BzPagination @bind-Page="page" TotalPages="12" Sibling="1" />

@code { int page = 1; }
```

#### BzTabs

```razor
<BzTabs @bind-ActiveIndex="tab" Variant="BzTabsVariant.Line" Orientation="BzOrientation.Horizontal">
    <BzTab Title="Profile" Icon="fa-solid fa-user">Profile content</BzTab>
    <BzTab Title="Settings" Icon="fa-solid fa-gear" Badge="3">Settings content</BzTab>
    <BzTab Title="Locked" Icon="fa-solid fa-lock" Disabled="true">Hidden</BzTab>
</BzTabs>

@code { int tab; }
```

`Variant`: Line, Pills, Boxed.

#### BzAccordion

```razor
<BzAccordion Multiple="false" Variant="BzAccordionVariant.Separated">
    <BzAccordionItem Title="Shipping" Icon="fa-solid fa-truck" Expanded="true">
        Delivery in 2-3 business days.
    </BzAccordionItem>
    <BzAccordionItem Title="Returns" Icon="fa-solid fa-rotate-left">
        30-day return policy.
    </BzAccordionItem>
</BzAccordion>
```

`Variant`: Default, Separated, Bordered, Filled, Minimal.

#### BzWizard

Multi-step flow with per-step validation, optional steps and built-in buttons.

```razor
<BzWizard @ref="wiz" Variant="BzWizardVariant.Numbered" Linear="true"
          ShowDefaultButtons="true" ShowCancel="true"
          OnFinish="OnFinish" OnCancel="OnCancel">
    <BzStep Title="Info" Subtitle="Your name" Icon="fa-solid fa-user" OnValidate="ValidateStep1">
        <BzInput TValue="string" @bind-Value="name" Label="Name" />
    </BzStep>
    <BzStep Title="Address" Icon="fa-solid fa-location-dot">
        <BzInput TValue="string" @bind-Value="address" Label="Address" />
    </BzStep>
    <BzStep Title="Payment" Icon="fa-solid fa-credit-card" Optional="true">
        <BzInput TValue="decimal?" @bind-Value="amount" Label="Amount"
                 Mode="BzInputMode.Money" Currency="BzCurrency.Toman" />
    </BzStep>
</BzWizard>

@code {
    BzWizard wiz = default!;
    string? name, address; decimal? amount;
    Task<bool> ValidateStep1() => Task.FromResult(!string.IsNullOrWhiteSpace(name));
    void OnFinish() { /* submit */ }
    void OnCancel() => wiz.SetMessage("Cancelled", BzSeverity.Warning);
}
```

`Variant`: Numbered, Dots, Progress, Arrows, Panels.

#### BzTimeline

```razor
<BzTimeline Items="events" Orientation="BzOrientation.Vertical" Variant="BzTimelineVariant.Alternate" />

@code {
    List<BzTimelineItem> events = new()
    {
        new("Order placed", "We received your order", "10:00", "fa-solid fa-cart-shopping", BzColor.Primary),
        new("Shipped", "On the way", "14:30", "fa-solid fa-truck", BzColor.Info),
        new("Delivered", "Enjoy!", "18:05", "fa-solid fa-check", BzColor.Success),
    };
}
```

`Variant`: Default, Alternate, Filled, Outlined, Minimal, Gradient.

### Overlays

#### Dialogs (BzDialogService)

Inject `IBzDialogService` and call it from code. Make sure `<BzDialogHost />` is in your layout (see setup).

```razor
@inject IBzDialogService Dialog

<BzButton Text="Delete" Color="BzColor.Danger" OnClick="Confirm" />

@code {
    async Task Confirm()
    {
        var ok = await Dialog.ConfirmAsync("Delete item", "Are you sure? This can't be undone.",
                                           okText: "Delete", cancelText: "Cancel");
        if (ok) { /* delete */ }

        await Dialog.AlertAsync("Done", "The item was deleted.");
    }
}
```

For a custom dialog, build `BzDialogOptions` and call `ShowAsync`:

```razor
@code {
    async Task ShowCustom()
    {
        var result = await Dialog.ShowAsync(new BzDialogOptions
        {
            Title = "New folder",
            Message = "Enter a name",
            Icon = "fa-solid fa-folder-plus",
            IconColor = BzColor.Primary,
            Size = BzDialogSize.Medium,
            CloseOnBackdrop = true,
            ShowCloseButton = true,
        });
    }
}
```

`Size`: Small, Medium, Large, ExtraLarge, FullScreen.

#### BzDrawer

A side panel that slides in from any edge.

```razor
<BzButton Text="Open drawer" OnClick="@(() => open = true)" />

<BzDrawer @bind-Open="open" Side="BzDrawerSide.End" Size="360px" Title="Filters" ShowClose="true">
    <ChildContent>
        <BzInput TValue="string" @bind-Value="q" Label="Search" />
    </ChildContent>
    <Footer>
        <BzButton Text="Apply" Color="BzColor.Primary" FullWidth />
    </Footer>
</BzDrawer>

@code { bool open; string? q; }
```

`Side`: Start, End, Top, Bottom.

#### BzPopup

A click-triggered popover.

```razor
<BzPopup @bind-Open="open" Placement="BzPopupPlacement.Bottom" Width="240px" CloseOnOutsideClick="true">
    <Trigger>
        <BzButton Text="Options" StartIcon="fa-solid fa-ellipsis" />
    </Trigger>
    <ChildContent>
        <BzButton Text="Edit" Variant="BzButtonVariant.Text" FullWidth />
        <BzButton Text="Delete" Variant="BzButtonVariant.Text" Color="BzColor.Danger" FullWidth />
    </ChildContent>
</BzPopup>

@code { bool open; }
```

`Placement`: Bottom, Top, Start, End.

#### BzContextMenu

Right-click menu over any content.

```razor
<BzContextMenu Items="items">
    <div style="padding:2rem; border:1px dashed var(--bz-border)">Right-click me</div>
</BzContextMenu>

@code {
    List<BzMenuItem> items = new()
    {
        new() { Text = "Copy", Icon = "fa-solid fa-copy" },
        new() { Text = "Paste", Icon = "fa-solid fa-paste" },
        new() { Text = "Delete", Icon = "fa-solid fa-trash", OnClick = () => { } },
    };
}
```

#### BzLightbox

A zoomable image gallery.

```razor
<BzLightbox Images="images" />

@code {
    List<string> images = new() { "/img/1.jpg", "/img/2.jpg", "/img/3.jpg" };
}
```

#### BzCarousel

A slider built on Swiper (load Swiper's JS/CSS in your host page).

```razor
<BzCarousel Slides="slides" Autoplay="3000" Loop="true"
            Pagination="true" Navigation="true" PerView="1"
            Effect="BzCarouselEffect.Slide" />

@code {
    List<RenderFragment> slides = new()
    {
        @<div class="slide">Slide 1</div>,
        @<div class="slide">Slide 2</div>,
        @<div class="slide">Slide 3</div>,
    };
}
```

`Effect`: Slide, Fade, Coverflow, Cube, Flip, Cards.

### Feedback

#### Notifications (BzNotificationService)

Inject `IBzNotificationService` and call the shortcuts. Needs `<BzNotificationHost />` in your layout.

```razor
@inject IBzNotificationService Toast

<BzButton Text="Save" OnClick="Save" />

@code {
    async Task Save()
    {
        Toast.Success("Saved successfully", title: "Done");
        Toast.Info("Heads up");
        Toast.Warning("Careful");
        Toast.Error("Something went wrong");

        // Wrap an async operation: shows pending, then success/error automatically
        await Toast.RunAsync(async () =>
        {
            await Task.Delay(1000);
        }, new BzRunOptions { Pending = "Saving...", Success = "All set" });
    }
}
```

Set position via `Toast.Position = BzToastPosition.TopEnd;` (TopEnd, TopStart, TopCenter, BottomEnd, BottomStart, BottomCenter).

#### BzTooltip

```razor
<BzTooltip Text="Delete permanently" Placement="BzTooltipPlacement.Top">
    <BzButton IconOnly StartIcon="fa-solid fa-trash" Color="BzColor.Danger" />
</BzTooltip>
```

`Placement`: Top, Bottom, Start, End.

#### BzOperationMessage

A running log of operation results (grouped, timestamped, dismissible).

```razor
<BzOperationMessage Items="ops" Grouped="true" Title="Activity"
                    ShowIcon="true" ShowTime="true" Dismissible="true" Max="20" />

@code {
    List<BzOperationItem> ops = new()
    {
        new() { Status = BzOpStatus.Success, Title = "Upload", Text = "file.pdf uploaded" },
        new() { Status = BzOpStatus.Warning, Text = "Low disk space" },
        new() { Status = BzOpStatus.Error,   Text = "Sync failed" },
    };
}
```

`Status`: Success, Error, Warning, Info, News.

#### BzSpinner

```razor
<BzSpinner Variant="BzSpinnerVariant.Ring" Size="48px" Color="BzColor.Primary" Label="Loading..." />
```

`Variant`: Ring, DualRing, Dots, Bars, Pulse, Grid, Ripple, Ellipsis, Roller, Wave, Orbit, Clock.

#### BzProgress

```razor
@* Linear *@
<BzProgress Type="BzProgressType.Linear" Value="65" Color="BzColor.Success"
            Striped Animated ShowLabel />

@* Circular, indeterminate *@
<BzProgress Type="BzProgressType.Circular" Indeterminate="true" Size="64px" Thickness="6px" />
```

`Type`: Linear, Circular.

#### BzSkeleton

```razor
<BzSkeleton Width="100%" Height="16px" Radius="8px" />
<BzSkeleton Circle="true" Width="48px" Height="48px" />
```

#### BzEmptyState

```razor
<BzEmptyState Icon="fa-solid fa-inbox" Title="No results"
              Description="Try changing your filters.">
    <Action>
        <BzButton Text="Reset filters" Variant="BzButtonVariant.Outlined" />
    </Action>
</BzEmptyState>
```

### Display

#### BzCard

```razor
<BzCard Variant="BzCardVariant.Elevated" Color="BzColor.Default"
        Title="Monthly report" Subtitle="July 2025" Icon="fa-solid fa-chart-line"
        Hoverable Clickable OnClick="OpenReport">
    <ChildContent>
        Revenue is up 12% versus last month.
    </ChildContent>
    <Actions>
        <BzButton Text="Open" Size="BzSize.Small" />
    </Actions>
</BzCard>

@code { void OpenReport() { } }
```

`Variant`: Elevated, Outlined, Filled, Soft, Gradient, Glass, AccentTop, AccentStart, Media, Horizontal.

#### BzStatCard

```razor
<BzStatCard Icon="fa-solid fa-users" Value="12,480" Label="Active users"
            Trend="+8.2%" TrendUp="true" Color="BzColor.Primary" Filled="false" Hoverable />
```

#### BzAvatar & BzAvatarGroup

```razor
<BzAvatar Text="HA" Size="48px" Shape="BzAvatarShape.Circle" Color="#2563eb" />
<BzAvatar ImageUrl="/img/user.jpg" Size="48px" Shape="BzAvatarShape.Rounded" />

<BzAvatarGroup>
    <BzAvatar Text="A" />
    <BzAvatar Text="B" />
    <BzAvatar Text="C" />
</BzAvatarGroup>
```

`Shape`: Circle, Rounded, Square.

#### BzBadge

```razor
<BzBadge Text="New" Icon="fa-solid fa-bolt" Variant="BzBadgeVariant.Soft" Color="BzColor.Success" />
```

`Variant`: Solid, Soft, Outline, Dot.

#### BzDivider

```razor
<BzDivider Text="OR" Orientation="BzDividerOrientation.Horizontal" />
```

#### BzUploader

Drag-and-drop file upload with size/count limits and previews.

```razor
<BzUploader Variant="BzUploaderVariant.Dropzone" Multiple="true"
            Accept="image/*,.pdf" MaxSizeMB="5" MaxFiles="10"
            Title="Drop files here" Hint="up to 5 MB each" ButtonText="Browse"
            OnFilesChanged="OnFiles" />

@code {
    void OnFiles(IReadOnlyList<BzUploadFile> files)
    {
        foreach (var f in files) Console.WriteLine($"{f.Name} - {f.SizeText}");
    }
}
```

`Variant`: Dropzone, Compact, Button, Avatar, Gallery.

#### BzEditor

Rich text editor (loads CKEditor 5 from your host page).

```razor
<BzEditor @bind-Value="html" Label="Description" Height="300px" ReadOnly="false" />

@code { string? html = "<p>Hello</p>"; }
```

#### BzFooter

```razor
<BzFooter Variant="BzFooterVariant.Columns" Copyright="© 2025 My Company">
    <ChildContent>
        <div><b>Product</b><br/>Features<br/>Pricing</div>
        <div><b>Company</b><br/>About<br/>Contact</div>
    </ChildContent>
    <Bottom>Made with Blazify</Bottom>
</BzFooter>
```

`Variant`: Simple, Columns, Dark, Gradient, Centered, Minimal.

### Iranian toolkit

Beyond `BzBankInput`, Blazify ships static helpers and DataAnnotations attributes.

Validation helpers (`BzCore.Helpers.IranValidation`):

```csharp
IranValidation.IsNationalCode("0012345678"); // bool
IranValidation.IsMobile("09121234567");       // bool
IranValidation.IsIban("IR820540102680020817909002"); // bool
IranValidation.GetIbanBank("IR8205...");      // bank name or null
IranValidation.IsBankCard("6037991234567890");// bool
IranValidation.GetCardBank("6037...");        // bank name or null
```

DataAnnotations attributes (`BzCore.Validation`) for use on your models:

```csharp
public class Customer
{
    [NationalCode] public string? NationalCode { get; set; }
    [IranMobile]   public string? Mobile { get; set; }
    [Sheba]        public string? Iban { get; set; }
    [BankCard]     public string? Card { get; set; }
}
```

Persian text helpers (`BzCore.Helpers.PersianHelper`):

```csharp
PersianHelper.ToLatinDigits("۱۲۳۴");   // "1234"
PersianHelper.ToPersianDigits("1234");  // Persian digits
PersianHelper.FixArabicChars("علي");    // normalizes Arabic ي/ك to Persian
```

Jalali calendar helpers (`BzCore.Helpers.CalendarHelper`):

```csharp
var (y, m, d) = CalendarHelper.GetParts(DateTime.Today, BzCalendarType.Jalali);
var dt        = CalendarHelper.ToDateTime(1404, 5, 15, BzCalendarType.Jalali);
var text      = CalendarHelper.FormatDate(DateTime.Today, BzCalendarType.Jalali, persianDigits: true);
```

## Theming & customization

Blazify is themed entirely through CSS variables, scoped under `BzThemeProvider`'s `.bz-scope` root.

**Built-in themes** (`BzThemeKind`): Light, Dark, LightGray, LightPurple.
**Appearances** (`BzAppearance`): Modern
**Fonts** (`BzFont`): IranSans

### Change theme, direction and font at runtime

Inject `IBzThemeService`:

```razor
@inject IBzThemeService Theme

<BzButton Text="Dark"  OnClick="@(() => Theme.SetTheme(BzThemeKind.Dark))" />
<BzButton Text="Light" OnClick="@(() => Theme.SetTheme(BzThemeKind.Light))" />
<BzButton Text="Modern" OnClick="@(() => Theme.SetAppearance(BzAppearance.Modern))" />
<BzButton Text="Toggle RTL/LTR" OnClick="@(() => Theme.ToggleDirection())" />
<BzButton Text="Vazirmatn" OnClick="@(() => Theme.SetFont(BzFont.Vazirmatn))" />
<BzButton Text="Compact" OnClick="@(() => Theme.SetDense(true))" />
<BzButton Text="Comfortable" OnClick="@(() => Theme.SetDense(false))" />
```

Every component inherits the global density setting. Set `Dense="false"` on an
individual component to opt it out of compact mode.

### Custom colors and radius

Override the CSS variables anywhere below `.bz-scope`. The main tokens:

```css
.bz-scope {
    --bz-primary:        #4f46e5;
    --bz-secondary:      #64748b;
    --bz-success:        #16a34a;
    --bz-info:           #0ea5e9;
    --bz-warning:        #f59e0b;
    --bz-danger:         #dc2626;

    --bz-background:     #f8fafc;
    --bz-surface:        #ffffff;
    --bz-border:         #e2e8f0;
    --bz-text:           #1e293b;
    --bz-text-muted:     #64748b;

    --bz-radius:         12px;
    --bz-radius-sm:      8px;
    --bz-radius-lg:      18px;
    --bz-control-h:      42px;
    --bz-font-size:      14px;
}
```

You can also pass a fully custom `BzTheme` object to `AddBlazify` (via `options.Theme`) or to `Theme.SetTheme(theme)` at runtime.

## Localization

Built-in languages: Persian (fa), Arabic (ar) and English (en). Switching language also flips direction automatically.

```razor
@inject IBzLocalizer L

<BzButton Text="فارسی"  OnClick="@(() => L.SetLanguage(BzLanguage.Fa))" />
<BzButton Text="العربية" OnClick="@(() => L.SetLanguage(BzLanguage.Ar))" />
<BzButton Text="English" OnClick="@(() => L.SetLanguage(BzLanguage.En))" />
```

## License

Free to use in personal and commercial projects. The bundled `LICENSE` file ships with the package. For questions, docs and live demos, visit **[blazify.ir](https://blazify.ir)**.

**[blazify.ir](https://blazify.ir)**

<br/>

---
---

<div dir="rtl">

# Blazify

**یک کتابخانه کامپوننت رابط کاربری برای Blazor؛ مدرن، قابل شخصی سازی و RTL-First برای .NET 8 و .NET 10.**

بیش از 50 کامپوننت با CSS و SVG خالص — دیتا گرید، ورودی ها، انتخابگر تاریخ و زمان، تقویم جلالی (شمسی)، دیالوگ، نمودار، نویگیشن بار، فرم و خیلی چیزهای دیگه — با پشتیبانی درجه یک از راست به چپ، چندزبانگی fa/ar/en و تم بندی مبتنی بر Tailwind. روی Blazor Server، WebAssembly و مدل یکپارچه Blazor Web App کار می کنه.

وب سایت: **[blazify.ir](https://blazify.ir)**

> استفاده اش توی پروژه های شخصی و تجاری رایگانه. بدون حساب کاربری و بدون کلید لایسنس. مستندات و دموی زنده توی [blazify.ir](https://blazify.ir) هست.

**زبان:** [English](#blazify) · فارسی (همین پایین)

---

## فهرست

- [چرا Blazify](#chera)
- [پیش نیاز ها](#pishniaz)
- [شروع سریع (قدم به قدم)](#shoru)
- [کامپوننت ها](#componentha)
- [تم بندی و شخصی سازی](#theming-fa)
- [چندزبانگی](#localization-fa)
- [لایسنس](#license-fa)

---

## چرا Blazify

- **RTL-First.** همه کامپوننت ها از روز اول برای RTL و LTR ساخته شدن و از CSS Logical Properties استفاده می کنن. جهت رو هم می تونی در لحظه با یک فراخوانی عوض کنی.
- **بیش از 50 کامپوننت.** گرید، ورودی ها، انتخابگر تاریخ و زمان، تقویم جلالی کامل، دیالوگ، نمودار SVG، منو، ویزارد، فرم و کلی چیز دیگه.
- **سبک.** تنها وابستگی اجباری اش `Microsoft.AspNetCore.Components.Web` هست. استایل CSS خالصه و آیکن ها SVG هستن.
- **قابل تم.** سیستم رنگ مبتنی بر Tailwind با تم های Light، Dark، LightGray و LightPurple، به علاوه ظاهرهای Modern . همه چیز با متغیرهای CSS کنترل می شه که خودت می تونی بازنویسی کنی.
- **چندزبانه.** فارسی (fa)، عربی (ar) و انگلیسی (en) به صورت داخلی، همراه با تغییر خودکار جهت.
- **آماده برای ایران.** اعتبارسنجی کد ملی، موبایل، شبا و شماره کارت، تقویم جلالی، مدیریت ارقام فارسی و تبدیل عدد به حروف.

## پیش نیازها

- .NET 8.0 یا .NET 10.0
- یکی از Blazor Server، Blazor WebAssembly یا Blazor Web App
- یک صفحه میزبان که بتونی توش تگ های `<link>` و `<script>` اضافه کنی (`App.razor`، `index.html` یا `_Host.cshtml`)

## شروع سریع

### 1. نصب پکیج

```bash
dotnet add package Blazify
```

یا از طریق Package Manager Console:

```powershell
Install-Package Blazify
```

### 2. ثبت سرویس ها

توی `Program.cs` متد `AddBlazify()` رو صدا بزن. می تونی آپشن بدی یا خالی بذاری تا مقادیر پیش فرض استفاده بشن.

```csharp
using BzCore.Extensions;
using BzCore.Enums;
using BzCore.Theming;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazify(options =>
{
    options.Appearance   = BzAppearance.Modern;  // Modern
    options.DefaultTheme = BzThemeKind.Light;    // Light | Dark | LightGray | LightPurple
    options.Direction    = BzDirection.Rtl;      // Rtl | Ltr
    options.Font         = BzFont.IranSans;      // IranSans
    options.Dense        = true;                  // فاصله‌گذاری فشرده در کل Blazify
});
```

اگه مقادیر پیش فرض برات کافیه، همینم کافیه:

```csharp
builder.Services.AddBlazify();
```

این کار سرویس های `IBzThemeService`، `IBzDialogService`، `IBzNotificationService` و `IBzLocalizer` رو ثبت می کنه.

### 3. اضافه کردن استایل و اسکریپت به صفحه میزبان

فایل‌های Blazify زیر مسیر `_content/Blazify` قرار دارن. یک فایل استایل و یک فایل اسکریپت رو به صفحه میزبانت (`App.razor`، `index.html` یا `_Host.cshtml`) اضافه کن. حتما همین مسیرهای با حروف کوچک رو بذار، چون روی هاست لینوکس به حروف بزرگ و کوچک حساسه.

توی `<head>`:

```html
<link rel="stylesheet" href="/_content/Blazify/css/blazify.css?v=1.2.0" />
```

قبل از بسته شدن `</body>` (بعد از `blazor.*.js`):

```html
<script src="/_content/Blazify/js/blazify.js?v=1.2.0"></script>
```

فونت‌های Blazify و Font Awesome داخل همین CSS قرار دارن. URLهای نسخه‌دار باعث کش شدن Bundleها می‌شن؛ در ASP.NET Core می‌شه برای مسیر `/_content/Blazify` هدر `Cache-Control` طولانی‌مدت هم تنظیم کرد. اتصال‌های اختیاری مثل CKEditor همچنان به اسکریپت خود آن کتابخانه نیاز دارن.

### 4. اضافه کردن Namespace ها

توی `_Imports.razor` هر Namespace ای که لازم داری رو اضافه کن. توجه کن که Namespace ریشه `BzCore` هست، نه `Blazify.Core`.

```razor
@using BzCore
@using BzCore.Enums
@using BzCore.Theming
@using BzCore.Helpers
@using BzCore.Localization
@using BzCore.Components.Button
@using BzCore.Components.Input
@using BzCore.Components.Form
@using BzCore.Components.Grid
@using BzCore.Components.DatePicker
@using BzCore.Components.Dialog
@using BzCore.Components.Notification
@using BzCore.Components.Navigation
@using BzCore.Components.Display
@using BzCore.Components.Overlay
@* ...بقیه رو هم هرموقع لازم شد اضافه کن *@
```

### 5. پیچیدن Layout و اضافه کردن Host ها

کامپوننت `BzThemeProvider` اسکوپ تم، جهت و متغیرهای CSS رو تنظیم می کنه. اون رو دور Layout ات بذار و `BzDialogHost` و `BzNotificationHost` رو یک بار داخلش قرار بده (تا دیالوگ ها و اعلان ها جایی برای رندر شدن داشته باشن).

```razor
@* MainLayout.razor *@
@inherits LayoutComponentBase

<BzThemeProvider>
    <main>
        @Body
    </main>

    <BzDialogHost />
    <BzNotificationHost />
</BzThemeProvider>
```

تمومه — حالا آماده ای که از کامپوننت ها استفاده کنی.

### 6. وابستگی های اختیاری (فقط برای چندتا کامپوننت)

بیشتر کامپوننت ها به چیز اضافه ای نیاز ندارن. فقط چند تاشون از کتابخانه های خارجی معروف استفاده می کنن که باید از CDN توی صفحه میزبانت لود کنی:

| کامپوننت | این رو توی صفحه میزبان لود کن |
| --- | --- |
| آیکن ها (توی کل کتابخانه با کلاس `fa-solid ...` استفاده می شن) | [Font Awesome](https://fontawesome.com) |
| `BzEditor` (متن با ادیتور) | CKEditor 5 (نسخه Classic) |
| `BzCarousel` | Swiper (هم JS و هم CSS) |

اگه کتابخانه ای لود نشده باشه، یه لاگ توی کنسول میزنه و قابل تشخیص اما کامپوننت های دیگه به خطا نمیخورن 

## کامپوننت ها

هر مثال پایین کامله و بعد از انجام مراحل نصب بالا، بدون تغییر اجرا می شه. هر پارامتری که توی مثال نیومده، مقدار پیش فرض معقول خودش رو داره.

### دکمه و ورودی

#### BzButton

```razor
<BzButton Text="Save"
          Color="BzColor.Primary"
          Variant="BzButtonVariant.Filled"
          Size="BzSize.Medium"
          StartIcon="fa-solid fa-floppy-disk"
          EndIcon="fa-solid fa-arrow-right"
          Loading="isSaving"
          AutoLoadingOnClick="true"
          LoadingEffect="BzLoadingEffect.Spinner"
          Ripple="true"
          Rounded="false"
          FullWidth="false"
          Disabled="false"
          ButtonType="button"
          OnClick="OnSave" />

@code {
    bool isSaving;
    async Task OnSave() { isSaving = true; await Task.Delay(800); isSaving = false; }
}
```

مقادیر `Color`: Default, Primary, Secondary, Success, Info, Warning, Danger, Surface · مقادیر `Variant`: Filled, Outlined, Text, Soft · مقادیر `Size`: Small, Medium, Large.

#### BzInput

یک ورودی متنی که حالت های متن ساده، مبلغ و ماسک، فیلتر کاراکتر و مدیریت ارقام فارسی رو پوشش می ده. روی نوع مقدار متصل (`TValue`) جنریک هست.

```razor
@* متن ساده با دکمه پاک کردن و آیکن *@
<BzInput TValue="string" @bind-Value="name"
         Label="Full name" Placeholder="Type your name"
         StartIcon="fa-solid fa-user" Clearable Required
         CharFilter="BzCharFilter.PersianAndEnglish"
         MaxLength="60" HelperText="As printed on your ID" />

@* ورودی مبلغ که عدد رو به حروف هم نشون می ده *@
<BzInput TValue="decimal?" @bind-Value="price"
         Label="Price" Mode="BzInputMode.Money"
         Currency="BzCurrency.Toman"
         ShowAmountInWords="true" AmountInWordsSuffix="Toman" />

@* ورودی ماسک دار (مثلا یک کد 10 رقمی) *@
<BzInput TValue="string" @bind-Value="code"
         Label="Code" Mode="BzInputMode.Mask" Mask="##########"
         ShowPersianDigits="false" UpdateTrigger="BzUpdateTrigger.OnInput" />

@code {
    string? name; decimal? price; string? code;
}
```

مقادیر `Mode`: Normal, Money, Mask · مقادیر `CharFilter`: Any, PersianOnly, EnglishOnly, PersianAndEnglish, NumberOnly, Decimal · مقادیر `Currency`: Rial, Toman, Dollar, Pound, Euro.

#### BzInputGroup

یک فیلد متنی با محتوای ابتدا/انتها و چند ظاهر مختلف.

```razor
<BzInputGroup @bind-Value="website"
              Label="Website" Placeholder="yoursite"
              Hint="Add your domain" Variant="BzInputGroupVariant.Outlined">
    <Prepend>https://</Prepend>
    <Append>.com</Append>
</BzInputGroup>

@code { string? website; }
```

مقادیر `Variant`: Default, Filled, Outlined, Pill, Underline, Seamless, Rounded.

#### BzAddonGroup

چند ورودی یا دکمه رو با افزونه های مشترک ابتدا/انتها کنار هم بذار.

```razor
<BzAddonGroup Variant="BzInputGroupVariant.Filled">
    <Prepend><i class="fa-solid fa-magnifying-glass"></i></Prepend>
    <ChildContent>
        <BzInput TValue="string" @bind-Value="term" Placeholder="Search..." />
    </ChildContent>
    <Append><BzButton Text="Go" Size="BzSize.Small" /></Append>
</BzAddonGroup>

@code { string? term; }
```

#### BzButtonGroup

یک مجموعه دکمه متصل که می تونه به شکل انتخاب تکی یا چندتایی کار کنه.

```razor
<BzButtonGroup TValue="string" @bind-Value="view"
               Variant="BzButtonGroupVariant.Toggle"
               Orientation="BzOrientation.Horizontal"
               Size="BzSize.Medium"
               Items="viewItems" />

@code {
    string? view = "grid";
    List<BzButtonGroupItem<string>> viewItems = new()
    {
        new("grid", "Grid", "fa-solid fa-table-cells"),
        new("list", "List", "fa-solid fa-list"),
        new("board","Board","fa-solid fa-columns"),
    };
}
```

مقادیر `Variant`: Attached, Separated, Pills, Outlined, Toggle. برای انتخاب چندتایی از `@bind-Values` و `Multiple` استفاده کن.

### کنترل های انتخاب

#### BzAutoComplete

جستجوی تایپی روی یک لیست ثابت یا یک تابع جستجوی async.

```razor
@* لیست ثابت *@
<BzAutoComplete TItem="string" @bind-Value="city"
                Label="City" Placeholder="Search city..."
                Items="cities" Clearable
                MinSearchLength="1" HighlightMatch="true" OpenOnFocus="true" />

@* جستجوی async *@
<BzAutoComplete TItem="string" @bind-Value="asyncCity"
                Label="City (async)" SearchFunc="SearchCitiesAsync"
                MinSearchLength="1" DebounceMs="250" MaxItems="20"
                NoResultsText="Nothing found" />

@code {
    string? city, asyncCity;
    string[] cities = { "Tehran", "Shiraz", "esfahan", "Tabriz" };
    Task<IEnumerable<string>> SearchCitiesAsync(string q)
        => Task.FromResult(cities.Where(c => c.Contains(q, StringComparison.OrdinalIgnoreCase)));
}
```

#### BzSelect

```razor
<BzSelect TValue="int" @bind-Value="cityId"
          Label="City" Placeholder="Choose..."
          Items="options" Clearable />

@code {
    int cityId;
    List<BzOption<int>> options = new()
    {
        new(1, "Tehran", "fa-solid fa-city"),
        new(2, "Shiraz"),
        new(3, "esfahan"),
    };
}
```

#### BzCheckbox

```razor
<BzCheckbox @bind-Value="agree" Label="I accept the terms"
            Color="BzColor.Primary" Shape="BzCheckShape.Rounded" Card="false" />

@code { bool agree; }
```

مقادیر `Shape`: Square, Rounded, Circle.

#### BzSwitch

```razor
<BzSwitch @bind-Value="enabled" Label="Notifications"
          Size="BzSize.Medium" Color="BzColor.Success" ShowIcons="true" />

@code { bool enabled = true; }
```

#### BzRadioGroup

```razor
<BzRadioGroup TValue="string" @bind-Value="plan"
              Items="plans" Orientation="BzOrientation.Vertical"
              Color="BzColor.Primary" Card="true" />

@code {
    string? plan = "pro";
    List<BzOption<string>> plans = new() { new("free","Free"), new("pro","Pro"), new("team","Team") };
}
```

#### BzSlider

```razor
<BzSlider @bind-Value="volume" Label="Volume"
          Min="0" Max="100" Step="5" ShowValue="true" />

@code { double volume = 40; }
```

#### BzRating

```razor
<BzRating @bind-Value="stars" Max="5" Icon="fa-solid fa-star"
          ShowValue="true" ReadOnly="false" />

@code { int stars = 3; }
```

#### BzNumberStepper

```razor
<BzNumberStepper @bind-Value="qty" Label="Quantity"
                 Min="1" Max="99" StepValue="1" />

@code { double qty = 1; }
```

#### BzColorPicker

```razor
<BzColorPicker @bind-Value="color" Label="Brand color" Swatches="swatches" />

@code {
    string? color = "#2563eb";
    string[] swatches = { "#2563eb", "#16a34a", "#dc2626", "#9333ea" };
}
```

#### BzTimePicker

```razor
<BzTimePicker @bind-Value="time" Label="Meeting time" Seconds="false" />

@code { string? time = "09:30"; }
```

#### BzTagInput

```razor
<BzTagInput @bind-Values="keywords" Label="Keywords" Placeholder="Type and press Enter" />

@code { List<string> keywords = new() { "blazor", "rtl" }; }
```

#### BzOtpInput

```razor
<BzOtpInput @bind-Value="otp" Length="6" />

@code { string? otp; }
```

#### BzFieldset

Group related fields with a title, icon and optional collapse.

```razor
<BzFieldset Title="Contact info" Icon="fa-solid fa-address-book"
            Variant="BzFieldsetVariant.Card" Color="BzColor.Primary"
            Collapsible="true" @bind-Collapsed="collapsed">
    <BzInput TValue="string" @bind-Value="email" Label="Email" />
    <BzInput TValue="string" @bind-Value="phone" Label="Phone" />
</BzFieldset>

@code { bool collapsed; string? email; string? phone; }
```

`Variant`: Legend, Card, Filled.

#### BzBankInput (Iranian identifiers)

یک فیلد برای شناسه های ایرانی، با گروه بندی، اعتبارسنجی زنده و تشخیص خودکار بانک.

```razor
@* شماره کارت: 16 رقم، گروه بندی شده، بانک رو تشخیص می ده *@
<BzBankInput Type="BzIdentifierType.CardNumber" @bind-Value="card"
             Label="Card number" ShowBank="true" ShowValidation="true"
             OnValidated="OnCardValidated" />

@* شبا: IR به همراه 24 رقم، با بررسی mod-97 و تشخیص بانک *@
<BzBankInput Type="BzIdentifierType.Iban" @bind-Value="iban"
             Label="Sheba (IBAN)" ShowBank="true" ShowValidation="true" />

@* کد ملی: 10 رقم *@
<BzBankInput Type="BzIdentifierType.NationalCode" @bind-Value="nationalCode"
             Label="National code" ShowValidation="true" />

@code {
    string? card, iban, nationalCode;
    void OnCardValidated(IdentifierValidationResult r) { /* r.IsValid, detected bank, etc. */ }
}
```

مقادیر `Type`: CardNumber, Iban, AccountNumber, NationalCode, LegalId, ForeignerId, Passport.

### (گرید و نمودار)

#### BzGrid

`BzGrid` از مرتب‌سازی، فیلتر، صفحه‌بندی، دریافت داده از سرور، ویرایش درجا،
جزئیات، گروه‌بندی، خروجی و انتخاب تکی یا چندتایی دلخواه پشتیبانی می‌کند.
با یک شرط داینامیک می‌توانید رکوردهای نهایی یا مالی را کاملاً قفل کنید:

```razor
<BzGrid TItem="Order" Items="orders"
        Selectable MultiSelect
        @bind-SelectedItems="selected"
        RowDisabled="@(order => order.IsFinalized)"
        HeaderActions="actions">
    <Columns>
        <BzColumn TItem="Order" Title="کد" Field="@(order => order.Id)" />
        <BzColumn TItem="Order" Title="مبلغ" Field="@(order => order.Amount)" />
    </Columns>
</BzGrid>

@code {
    IReadOnlyCollection<Order> selected = [];

    IReadOnlyList<BzGridHeaderAction<Order>> actions =
    [
        new() { Title = "ایجاد", SelectionRequirement = BzGridActionSelectionRequirement.None },
        new() { Title = "مشاهده", SelectionRequirement = BzGridActionSelectionRequirement.ExactlyOne },
        new() { Title = "تأیید", SelectionRequirement = BzGridActionSelectionRequirement.OneOrMore }
    ];
}
```

ردیف غیرفعال با Select All نیز انتخاب نمی‌شود و کلیک، بازشدن جزئیات، ویرایش و
کنترل‌های داخل Template آن قابل اجرا نیستند. اکشن‌های Header هم تا زمان برقرارشدن
شرط تعداد انتخاب، غیرفعال باقی می‌مانند.

### تاریخ و زمان

#### BzDatePicker

جلالی (شمسی) یا میلادی، تک تاریخ یا بازه، با ارقام فارسی اختیاری و بازه بدون سقف.

```razor
@* تک تاریخ جلالی *@
<BzDatePicker @bind-Value="date"
              Calendar="BzCalendarType.Jalali" PersianDigits="true"
              Min="DateTime.Today.AddYears(-1)" Max="DateTime.Today.AddYears(1)" />

@* بازه با انتهای اختیاری «تا اطلاع ثانوی» *@
<BzDatePicker @bind-Value="from" @bind-EndValue="to"
              Range="true" AllowUntilFurtherNotice="true"
              Calendar="BzCalendarType.Jalali" />

@code { DateTime? date; DateTime? from; DateTime? to; }
```

مقادیر `Calendar`: Jalali, Gregorian.

#### BzCalendarView

نمای ماهانه درون صفحه ای همراه با رویدادها.

```razor
<BzCalendarView @bind-SelectedDate="day"
                Calendar="BzCalendarType.Jalali" PersianDigits="true"
                ShowHolidays="true" Events="events"
                OnDayClick="d => day = d" />

@code {
    DateTime? day = DateTime.Today;
    List<BzCalendarEvent> events = new()
    {
        new(DateTime.Today, "Team sync", BzColor.Primary),
        new(DateTime.Today.AddDays(2), "Release", BzColor.Success),
    };
}
```

#### BzCalendarFull

یک تقویم تمام صفحه با چند رویداد در هر روز.

```razor
<BzCalendarFull @bind-SelectedDate="day"
                Calendar="BzCalendarType.Jalali" PersianDigits="true"
                ShowHolidays="true" MaxEventsPerDay="3" Events="events" />

@code {
    DateTime? day = DateTime.Today;
    List<BzCalendarEvent> events = new() { new(DateTime.Today, "Standup", BzColor.Info) };
}
```

### نویگیشن بار

#### BzNavbar

```razor
<BzNavbar Variant="BzNavbarVariant.Glass" Sticky="true">
    <Brand><i class="fa-solid fa-cubes"></i> My App</Brand>
    <Center>
        <BzMenu Items="menu" Orientation="BzOrientation.Horizontal" />
    </Center>
    <End>
        <BzButton Text="Sign in" Size="BzSize.Small" Variant="BzButtonVariant.Soft" />
    </End>
</BzNavbar>

@code {
    List<BzMenuItem> menu = new()
    {
        new() { Text = "Home", Href = "/" },
        new() { Text = "Docs", Href = "/docs" },
    };
}
```

مقادیر `Variant`: Solid, Light, Dark, Gradient, Glass, Bordered, Elevated, Minimal, Accent, Centered.

#### BzMenu

از زیرآیتم های تودرتو، آیکن، بج و اکشن کلیک پشتیبانی می کنه.

```razor
<BzMenu Items="items" Variant="BzMenuVariant.Pills" Orientation="BzOrientation.Horizontal" />

@code {
    List<BzMenuItem> items = new()
    {
        new() { Text = "Dashboard", Icon = "fa-solid fa-gauge", Href = "/" },
        new() { Text = "Products",  Icon = "fa-solid fa-box", Badge = "New",
                Children = new() {
                    new() { Text = "All", Href = "/products" },
                    new() { Text = "Add", Href = "/products/new" },
                }},
        new() { Text = "Logout", Icon = "fa-solid fa-right-from-bracket",
                OnClick = () => Console.WriteLine("bye") },
    };
}
```

مقادیر `Variant`: Underline, Pills, Solid, Soft, Bordered, Minimal, Boxed, Accent, Gradient, Dark.

#### BzBreadcrumbs

```razor
<BzBreadcrumbs Items="crumbs" Separator="BzBreadcrumbSeparator.Chevron" />

@code {
    List<BzBreadcrumbItem> crumbs = new()
    {
        new() { Text = "Home", Icon = "fa-solid fa-house", Href = "/" },
        new() { Text = "Products", Href = "/products" },
        new() { Text = "Details" },
    };
}
```

مقادیر `Separator`: Chevron, Slash, Arrow, Dot.

#### BzSidebar

یک پنل کناری جمع شونده. اون رو با `BzNavGroup` و `BzNavItem` بساز.

```razor
<BzSidebar @bind-Collapsed="collapsed" Width="260px" CollapsedWidth="72px"
           Variant="BzSidebarVariant.Floating" ShowToggle="true">
    <HeaderContent><b>My App</b></HeaderContent>
    <ChildContent>
        <BzNavGroup Title="Main">
            <BzNavItem Text="Dashboard" Icon="fa-solid fa-gauge" Href="/" />
            <BzNavItem Text="Orders" Icon="fa-solid fa-cart-shopping" Href="/orders" BadgeText="8" />
        </BzNavGroup>
        <BzNavGroup Title="Settings">
            <BzNavItem Text="Profile" Icon="fa-solid fa-user" Href="/profile" />
        </BzNavGroup>
    </ChildContent>
    <FooterContent>v1.0.0</FooterContent>
</BzSidebar>

@code { bool collapsed; }
```

مقادیر `Variant`: Default, Dark, Primary, Glass, Floating.

#### BzFloatingBox

یک باکس اکشن شناور که به یک گوشه صفحه چسبیده.

```razor
<BzFloatingBox Icon="fa-solid fa-headset" Title="Support"
               Position="BzFloatingPosition.BottomEnd" @bind-Open="open">
    <p>How can we help?</p>
    <BzButton Text="Start chat" Size="BzSize.Small" />
</BzFloatingBox>

@code { bool open; }
```

مقادیر `Position`: BottomEnd, BottomStart, TopEnd, TopStart.

#### BzTreeView

درخت جنریک با انتخاب، جستجو و اکشن به ازای هر گره.

```razor
<BzTreeView TItem="Node" Items="roots"
            ChildrenSelector="n => n.Children"
            TextSelector="n => n.Name"
            IconSelector="n => n.Icon"
            Selectable MultiSelect Searchable
            ExpandedByDefault="false"
            OnNodeClick="n => Console.WriteLine(n.Name)" />

@code {
    List<Node> roots = new()
    {
        new("src", "fa-solid fa-folder", new()
        {
            new("Program.cs", "fa-solid fa-file-code"),
            new("wwwroot", "fa-solid fa-folder"),
        }),
    };
    public record Node(string Name, string? Icon = null, List<Node>? Children = null);
}
```

#### BzPagination

```razor
<BzPagination @bind-Page="page" TotalPages="12" Sibling="1" />

@code { int page = 1; }
```

#### BzTabs

```razor
<BzTabs @bind-ActiveIndex="tab" Variant="BzTabsVariant.Line" Orientation="BzOrientation.Horizontal">
    <BzTab Title="Profile" Icon="fa-solid fa-user">Profile content</BzTab>
    <BzTab Title="Settings" Icon="fa-solid fa-gear" Badge="3">Settings content</BzTab>
    <BzTab Title="Locked" Icon="fa-solid fa-lock" Disabled="true">Hidden</BzTab>
</BzTabs>

@code { int tab; }
```

مقادیر `Variant`: Line, Pills, Boxed.

#### BzAccordion

```razor
<BzAccordion Multiple="false" Variant="BzAccordionVariant.Separated">
    <BzAccordionItem Title="Shipping" Icon="fa-solid fa-truck" Expanded="true">
        Delivery in 2-3 business days.
    </BzAccordionItem>
    <BzAccordionItem Title="Returns" Icon="fa-solid fa-rotate-left">
        30-day return policy.
    </BzAccordionItem>
</BzAccordion>
```

مقادیر `Variant`: Default, Separated, Bordered, Filled, Minimal.

#### BzWizard

جریان چند مرحله ای با اعتبارسنجی هر مرحله، مرحله های اختیاری و دکمه های آماده.

```razor
<BzWizard @ref="wiz" Variant="BzWizardVariant.Numbered" Linear="true"
          ShowDefaultButtons="true" ShowCancel="true"
          OnFinish="OnFinish" OnCancel="OnCancel">
    <BzStep Title="Info" Subtitle="Your name" Icon="fa-solid fa-user" OnValidate="ValidateStep1">
        <BzInput TValue="string" @bind-Value="name" Label="Name" />
    </BzStep>
    <BzStep Title="Address" Icon="fa-solid fa-location-dot">
        <BzInput TValue="string" @bind-Value="address" Label="Address" />
    </BzStep>
    <BzStep Title="Payment" Icon="fa-solid fa-credit-card" Optional="true">
        <BzInput TValue="decimal?" @bind-Value="amount" Label="Amount"
                 Mode="BzInputMode.Money" Currency="BzCurrency.Toman" />
    </BzStep>
</BzWizard>

@code {
    BzWizard wiz = default!;
    string? name, address; decimal? amount;
    Task<bool> ValidateStep1() => Task.FromResult(!string.IsNullOrWhiteSpace(name));
    void OnFinish() { /* submit */ }
    void OnCancel() => wiz.SetMessage("Cancelled", BzSeverity.Warning);
}
```

مقادیر `Variant`: Numbered, Dots, Progress, Arrows, Panels.

#### BzTimeline

```razor
<BzTimeline Items="events" Orientation="BzOrientation.Vertical" Variant="BzTimelineVariant.Alternate" />

@code {
    List<BzTimelineItem> events = new()
    {
        new("Order placed", "We received your order", "10:00", "fa-solid fa-cart-shopping", BzColor.Primary),
        new("Shipped", "On the way", "14:30", "fa-solid fa-truck", BzColor.Info),
        new("Delivered", "Enjoy!", "18:05", "fa-solid fa-check", BzColor.Success),
    };
}
```

مقادیر `Variant`: Default, Alternate, Filled, Outlined, Minimal, Gradient.

### لایه ها و پنجره ها

#### Dialogs (BzDialogService)

سرویس `IBzDialogService` رو تزریق کن و از کد صداش بزن. مطمئن شو که `<BzDialogHost />` توی Layout ات هست (بخش نصب رو ببین).

```razor
@inject IBzDialogService Dialog

<BzButton Text="Delete" Color="BzColor.Danger" OnClick="Confirm" />

@code {
    async Task Confirm()
    {
        var ok = await Dialog.ConfirmAsync("Delete item", "Are you sure? This can't be undone.",
                                           okText: "Delete", cancelText: "Cancel");
        if (ok) { /* delete */ }

        await Dialog.AlertAsync("Done", "The item was deleted.");
    }
}
```

برای دیالوگ سفارشی، یک `BzDialogOptions` بساز و `ShowAsync` رو صدا بزن:

```razor
@code {
    async Task ShowCustom()
    {
        var result = await Dialog.ShowAsync(new BzDialogOptions
        {
            Title = "New folder",
            Message = "Enter a name",
            Icon = "fa-solid fa-folder-plus",
            IconColor = BzColor.Primary,
            Size = BzDialogSize.Medium,
            CloseOnBackdrop = true,
            ShowCloseButton = true,
        });
    }
}
```

مقادیر `Size`: Small, Medium, Large, ExtraLarge, FullScreen.

#### BzDrawer

یک پنل کناری که از هر لبه ای می تونه بیاد تو.

```razor
<BzButton Text="Open drawer" OnClick="@(() => open = true)" />

<BzDrawer @bind-Open="open" Side="BzDrawerSide.End" Size="360px" Title="Filters" ShowClose="true">
    <ChildContent>
        <BzInput TValue="string" @bind-Value="q" Label="Search" />
    </ChildContent>
    <Footer>
        <BzButton Text="Apply" Color="BzColor.Primary" FullWidth />
    </Footer>
</BzDrawer>

@code { bool open; string? q; }
```

مقادیر `Side`: Start, End, Top, Bottom.

#### BzPopup

یک پاپ اور که با کلیک باز می شه.

```razor
<BzPopup @bind-Open="open" Placement="BzPopupPlacement.Bottom" Width="240px" CloseOnOutsideClick="true">
    <Trigger>
        <BzButton Text="Options" StartIcon="fa-solid fa-ellipsis" />
    </Trigger>
    <ChildContent>
        <BzButton Text="Edit" Variant="BzButtonVariant.Text" FullWidth />
        <BzButton Text="Delete" Variant="BzButtonVariant.Text" Color="BzColor.Danger" FullWidth />
    </ChildContent>
</BzPopup>

@code { bool open; }
```

مقادیر `Placement`: Bottom, Top, Start, End.

#### BzContextMenu

منوی کلیک راست روی هر محتوایی.

```razor
<BzContextMenu Items="items">
    <div style="padding:2rem; border:1px dashed var(--bz-border)">Right-click me</div>
</BzContextMenu>

@code {
    List<BzMenuItem> items = new()
    {
        new() { Text = "Copy", Icon = "fa-solid fa-copy" },
        new() { Text = "Paste", Icon = "fa-solid fa-paste" },
        new() { Text = "Delete", Icon = "fa-solid fa-trash", OnClick = () => { } },
    };
}
```

#### BzLightbox

یک گالری تصویر با قابلیت زوم.

```razor
<BzLightbox Images="images" />

@code {
    List<string> images = new() { "/img/1.jpg", "/img/2.jpg", "/img/3.jpg" };
}
```

#### BzCarousel

یک اسلایدر مبتنی بر Swiper (فایل های JS/CSS مربوط به Swiper رو توی صفحه میزبان لود کن).

```razor
<BzCarousel Slides="slides" Autoplay="3000" Loop="true"
            Pagination="true" Navigation="true" PerView="1"
            Effect="BzCarouselEffect.Slide" />

@code {
    List<RenderFragment> slides = new()
    {
        @<div class="slide">Slide 1</div>,
        @<div class="slide">Slide 2</div>,
        @<div class="slide">Slide 3</div>,
    };
}
```

مقادیر `Effect`: Slide, Fade, Coverflow, Cube, Flip, Cards.

### بازخورد و وضعیت

#### Notifications (BzNotificationService)

سرویس `IBzNotificationService` رو تزریق کن و از میانبرها استفاده کن. به `<BzNotificationHost />` توی Layout نیاز داره.

```razor
@inject IBzNotificationService Toast

<BzButton Text="Save" OnClick="Save" />

@code {
    async Task Save()
    {
        Toast.Success("Saved successfully", title: "Done");
        Toast.Info("Heads up");
        Toast.Warning("Careful");
        Toast.Error("Something went wrong");

        // پیچیدن یک عملیات async: اول در حال انجام، بعد موفقیت/خطا به صورت خودکار
        await Toast.RunAsync(async () =>
        {
            await Task.Delay(1000);
        }, new BzRunOptions { Pending = "Saving...", Success = "All set" });
    }
}
```

موقعیت رو با `Toast.Position = BzToastPosition.TopEnd;` تنظیم کن (مقادیر TopEnd, TopStart, TopCenter, BottomEnd, BottomStart, BottomCenter).

#### BzTooltip

```razor
<BzTooltip Text="Delete permanently" Placement="BzTooltipPlacement.Top">
    <BzButton IconOnly StartIcon="fa-solid fa-trash" Color="BzColor.Danger" />
</BzTooltip>
```

مقادیر `Placement`: Top, Bottom, Start, End.

#### BzOperationMessage

یک لاگ زنده از نتیجه عملیات ها (گروه بندی شده، زمان دار و قابل بستن).

```razor
<BzOperationMessage Items="ops" Grouped="true" Title="Activity"
                    ShowIcon="true" ShowTime="true" Dismissible="true" Max="20" />

@code {
    List<BzOperationItem> ops = new()
    {
        new() { Status = BzOpStatus.Success, Title = "Upload", Text = "file.pdf uploaded" },
        new() { Status = BzOpStatus.Warning, Text = "Low disk space" },
        new() { Status = BzOpStatus.Error,   Text = "Sync failed" },
    };
}
```

مقادیر `Status`: Success, Error, Warning, Info, News.

#### BzSpinner

```razor
<BzSpinner Variant="BzSpinnerVariant.Ring" Size="48px" Color="BzColor.Primary" Label="Loading..." />
```

مقادیر `Variant`: Ring, DualRing, Dots, Bars, Pulse, Grid, Ripple, Ellipsis, Roller, Wave, Orbit, Clock.

#### BzProgress

```razor
@* خطی *@
<BzProgress Type="BzProgressType.Linear" Value="65" Color="BzColor.Success"
            Striped Animated ShowLabel />

@* دایره ای و نامعین *@
<BzProgress Type="BzProgressType.Circular" Indeterminate="true" Size="64px" Thickness="6px" />
```

مقادیر `Type`: Linear, Circular.

#### BzSkeleton

```razor
<BzSkeleton Width="100%" Height="16px" Radius="8px" />
<BzSkeleton Circle="true" Width="48px" Height="48px" />
```

#### BzEmptyState

```razor
<BzEmptyState Icon="fa-solid fa-inbox" Title="No results"
              Description="Try changing your filters.">
    <Action>
        <BzButton Text="Reset filters" Variant="BzButtonVariant.Outlined" />
    </Action>
</BzEmptyState>
```

### نمایش و محتوا

#### BzCard

```razor
<BzCard Variant="BzCardVariant.Elevated" Color="BzColor.Default"
        Title="Monthly report" Subtitle="July 2025" Icon="fa-solid fa-chart-line"
        Hoverable Clickable OnClick="OpenReport">
    <ChildContent>
        Revenue is up 12% versus last month.
    </ChildContent>
    <Actions>
        <BzButton Text="Open" Size="BzSize.Small" />
    </Actions>
</BzCard>

@code { void OpenReport() { } }
```

مقادیر `Variant`: Elevated, Outlined, Filled, Soft, Gradient, Glass, AccentTop, AccentStart, Media, Horizontal.

#### BzStatCard

```razor
<BzStatCard Icon="fa-solid fa-users" Value="12,480" Label="Active users"
            Trend="+8.2%" TrendUp="true" Color="BzColor.Primary" Filled="false" Hoverable />
```

#### BzAvatar & BzAvatarGroup

```razor
<BzAvatar Text="HA" Size="48px" Shape="BzAvatarShape.Circle" Color="#2563eb" />
<BzAvatar ImageUrl="/img/user.jpg" Size="48px" Shape="BzAvatarShape.Rounded" />

<BzAvatarGroup>
    <BzAvatar Text="A" />
    <BzAvatar Text="B" />
    <BzAvatar Text="C" />
</BzAvatarGroup>
```

`Shape`: Circle, Rounded, Square.

#### BzBadge

```razor
<BzBadge Text="New" Icon="fa-solid fa-bolt" Variant="BzBadgeVariant.Soft" Color="BzColor.Success" />
```

مقادیر `Variant`: Solid, Soft, Outline, Dot.

#### BzDivider

```razor
<BzDivider Text="OR" Orientation="BzDividerOrientation.Horizontal" />
```

#### BzUploader

آپلود فایل با کشیدن و رها کردن، همراه با محدودیت حجم/تعداد و پیش نمایش.

```razor
<BzUploader Variant="BzUploaderVariant.Dropzone" Multiple="true"
            Accept="image/*,.pdf" MaxSizeMB="5" MaxFiles="10"
            Title="Drop files here" Hint="up to 5 MB each" ButtonText="Browse"
            OnFilesChanged="OnFiles" />

@code {
    void OnFiles(IReadOnlyList<BzUploadFile> files)
    {
        foreach (var f in files) Console.WriteLine($"{f.Name} - {f.SizeText}");
    }
}
```

مقادیر `Variant`: Dropzone, Compact, Button, Avatar, Gallery.

#### BzEditor

ویرایشگر متن (CKEditor 5 رو از صفحه میزبان لود می کنه).

```razor
<BzEditor @bind-Value="html" Label="Description" Height="300px" ReadOnly="false" />

@code { string? html = "<p>Hello</p>"; }
```

#### BzFooter

```razor
<BzFooter Variant="BzFooterVariant.Columns" Copyright="© 2026 Blazify">
    <ChildContent>
        <div><b>Product</b><br/>Features<br/>Pricing</div>
        <div><b>Company</b><br/>About<br/>Contact</div>
    </ChildContent>
    <Bottom>Made with Blazify</Bottom>
</BzFooter>
```

مقادیر `Variant`: Simple, Columns, Dark, Gradient, Centered, Minimal.

### ابزار ایرانی 

علاوه بر `BzBankInput`، خود Blazify هم هلپرهای استاتیک و اتریبیوت های DataAnnotations داره.

هلپرهای اعتبارسنجی (`BzCore.Helpers.IranValidation`):

```csharp
IranValidation.IsNationalCode("0012345678"); // bool
IranValidation.IsMobile("09121234567");       // bool
IranValidation.IsIban("IR820540102680020817909002"); // bool
IranValidation.GetIbanBank("IR8205...");      // نام بانک یا null
IranValidation.IsBankCard("6037991234567890");// bool
IranValidation.GetCardBank("6037...");        // نام بانک یا null
```

اتریبیوت های DataAnnotations (`BzCore.Validation`) برای استفاده روی مدل ها:

```csharp
public class Customer
{
    [NationalCode] public string? NationalCode { get; set; }
    [IranMobile]   public string? Mobile { get; set; }
    [Sheba]        public string? Iban { get; set; }
    [BankCard]     public string? Card { get; set; }
}
```

هلپرهای متن فارسی (`BzCore.Helpers.PersianHelper`):

```csharp
PersianHelper.ToLatinDigits("۱۲۳۴");   // "1234"
PersianHelper.ToPersianDigits("1234");  // ارقام فارسی
PersianHelper.FixArabicChars("علی");    // ی و ک عربی رو به فارسی نرمال می کنه
```

هلپرهای تقویم جلالی (`BzCore.Helpers.CalendarHelper`):

```csharp
var (y, m, d) = CalendarHelper.GetParts(DateTime.Today, BzCalendarType.Jalali);
var dt        = CalendarHelper.ToDateTime(1404, 5, 15, BzCalendarType.Jalali);
var text      = CalendarHelper.FormatDate(DateTime.Today, BzCalendarType.Jalali, persianDigits: true);
```

## تم بندی و شخصی سازی 

کل تم بندی Blazify از طریق متغیرهای CSS انجام می شه که زیر ریشه `.bz-scope` مربوط به `BzThemeProvider` اسکوپ شدن.

**تم های آماده** (`BzThemeKind`): Light, Dark, LightGray, LightPurple.
**ظاهرها** (`BzAppearance`): Modern
**فونت ها** (`BzFont`): IranSans

### تغییر تم، جهت و فونت در زمان اجرا

سرویس `IBzThemeService` رو تزریق کن:

```razor
@inject IBzThemeService Theme

<BzButton Text="Dark"  OnClick="@(() => Theme.SetTheme(BzThemeKind.Dark))" />
<BzButton Text="Light" OnClick="@(() => Theme.SetTheme(BzThemeKind.Light))" />
<BzButton Text="Modern" OnClick="@(() => Theme.SetAppearance(BzAppearance.Modern))" />
<BzButton Text="Toggle RTL/LTR" OnClick="@(() => Theme.ToggleDirection())" />
<BzButton Text="Vazirmatn" OnClick="@(() => Theme.SetFont(BzFont.Vazirmatn))" />
<BzButton Text="فشرده" OnClick="@(() => Theme.SetDense(true))" />
<BzButton Text="عادی" OnClick="@(() => Theme.SetDense(false))" />
```

همه‌ی کامپوننت‌ها تنظیم Dense سراسری را به ارث می‌برند. برای خارج‌کردن یک
کامپوننت خاص از حالت فشرده، روی همان کامپوننت `Dense="false"` قرار بده.

### رنگ و گردی گوشه سفارشی

متغیرهای CSS رو هر جایی زیر `.bz-scope` بازنویسی کن. توکن های اصلی:

```css
.bz-scope {
    --bz-primary:        #4f46e5;
    --bz-secondary:      #64748b;
    --bz-success:        #16a34a;
    --bz-info:           #0ea5e9;
    --bz-warning:        #f59e0b;
    --bz-danger:         #dc2626;

    --bz-background:     #f8fafc;
    --bz-surface:        #ffffff;
    --bz-border:         #e2e8f0;
    --bz-text:           #1e293b;
    --bz-text-muted:     #64748b;

    --bz-radius:         12px;
    --bz-radius-sm:      8px;
    --bz-radius-lg:      18px;
    --bz-control-h:      42px;
    --bz-font-size:      14px;
}
```

همچنین می تونی یک آبجکت `BzTheme` کاملا سفارشی رو به `AddBlazify` (از طریق `options.Theme`) یا در زمان اجرا به `Theme.SetTheme(theme)` بدی.

## چندزبانگی 

زبان های آماده: فارسی (fa)، و انگلیسی (en). با تغییر زبان، جهت هم به صورت خودکار عوض می شه.

```razor
@inject IBzLocalizer L

<BzButton Text="فارسی"  OnClick="@(() => L.SetLanguage(BzLanguage.Fa))" />
<BzButton Text="English" OnClick="@(() => L.SetLanguage(BzLanguage.En))" />
```

## لایسنس 

استفاده اش توی پروژه های شخصی و تجاری رایگانه. فایل `LICENSE` همراه پکیج ارائه می شه. برای سوال، مستندات و دموی زنده به **[blazify.ir](https://blazify.ir)** سر بزن.

 **[blazify.ir](https://blazify.ir)**

---

با علاقه برای جامعه Blazor و توسعه دهنده های فارسی زبان و RTL ساخته شده — **[blazify.ir](https://blazify.ir)**

