using System;
using Core.Common.Core;
using FluentValidation;

namespace CarRental.Client.Entities
{
    public class Car : ObjectBase
    {
        private int _carId;
        private string _description;
        private string _color;
        private int _year;
        private decimal _rentalPrice;
        private bool _currentlyRented;

        public int CarId
        {
            get { return _carId; }
            set
            {
                if (_carId != value)
                {
                    _carId = value;
                    OnPropertyChanged(() => CarId);
                }
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(() => Description);
                }
            }
        }

        public string Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged(() => Color);
                }
            }
        }

        public int Year
        {
            get { return _year; }
            set
            {
                if (_year != value)
                {
                    _year = value;
                    OnPropertyChanged(() => Year);
                }
            }
        }

        public decimal RentalPrice
        {
            get { return _rentalPrice; }
            set
            {
                if (_rentalPrice != value)
                {
                    _rentalPrice = value;
                    OnPropertyChanged(() => RentalPrice);
                }
            }
        }

        public bool CurrentlyRented
        {
            get { return _currentlyRented; }
            set
            {
                if (_currentlyRented != value)
                {
                    _currentlyRented = value;
                    OnPropertyChanged(() => CurrentlyRented);
                }
            }
        }

        class CarValidator : AbstractValidator<Car>
        {
            public CarValidator()
            {
                RuleFor(obj => obj.Description).NotEmpty();
                RuleFor(obj => obj.Color).NotEmpty();
                RuleFor(obj => obj.RentalPrice).GreaterThan(0);
                RuleFor(obj => obj.Year).GreaterThan(200).LessThanOrEqualTo(DateTime.Now.Year + 1);
            }
        }

        protected override IValidator GetValidator()
        {
            return new CarValidator();
        }
    }
}
