using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using SmartLanche.Data;
using SmartLanche.Helpers;
using SmartLanche.Messages;
using SmartLanche.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using static SmartLanche.Helpers.EnumValuesExtension;

namespace SmartLanche.ViewModels
{
    public partial class StockHistoryViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public StockHistoryViewModel(IDbContextFactory<AppDbContext> contextFactory, IMessenger messenger) : base(messenger)
        {
            _contextFactory = contextFactory;

            movements = new ObservableCollection<StockMovement>();
            MovementsView = CollectionViewSource.GetDefaultView(Movements);
            MovementsView.Filter = FilterMovements;

            InitializeFilters();

            Messenger.Register<StockHistoryViewModel, ProductsChangedMessage>(this, async (r, m) => await LoadMovementsAsync());            

            _ = LoadMovementsAsync();
        }

        #region Propriedades Observáveis

        public ICollectionView MovementsView { get; private set; }

        [ObservableProperty]
        private ObservableCollection<StockMovement> movements = new();

        [ObservableProperty]
        private DateTime? filterDate;

        [ObservableProperty]
        private EnumValue? selectedFilterType;

        [ObservableProperty]
        private MovementType? filterType;        

        [ObservableProperty]
        private List<EnumValue> filterTypes = new();

        #endregion       

        #region Comandos

        [RelayCommand]
        public async Task LoadMovementsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                using var context = await _contextFactory.CreateDbContextAsync();

                var list = await context.StockMovements
                    .Include(stockMoviment => stockMoviment.Product)
                    .OrderByDescending(stockMoviment => stockMoviment.Date)
                    .ToListAsync();

                Movements.Clear();

                foreach (var item in list) Movements.Add(item);

                MovementsView.Refresh();
            }
            catch (Exception ex)
            { 
                Messenger.Send(new StatusMessage($"Erro ao carregar histórico: {ex.Message}", false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            FilterDate = null;
            SelectedFilterType = FilterTypes.FirstOrDefault();
            MovementsView.Refresh();
        }

        #endregion

        #region Lógica de Apoio

        public void InitializeFilters()
        {
            var list = new List<EnumValue>();

            list.Add(new EnumValue { DisplayName = "Todos", Value = FilterOptions.All });

            var enumValues = new EnumValuesExtension(typeof(MovementType)).ProvideValue(null!) as List<EnumValue>;

            if (enumValues != null) list.AddRange(enumValues);

            FilterTypes = list;
            SelectedFilterType = list[0];
        }

        private bool FilterMovements(object obj)
        {
            if (obj is not StockMovement movement) return false;

            bool dateMatch = !FilterDate.HasValue || movement.Date.Date == FilterDate.Value.Date;
            
            bool typeMatch = true;
            if (SelectedFilterType?.Value is MovementType type)
            {
                typeMatch = movement.Type == type;
            }
            else if (SelectedFilterType?.Value is FilterOptions.All)
            {
                typeMatch = true;
            }

            return dateMatch && typeMatch;
        }

        partial void OnFilterDateChanged(DateTime? value) => MovementsView.Refresh();
        partial void OnSelectedFilterTypeChanged(EnumValue? value) => MovementsView.Refresh();

        #endregion
    }
}
