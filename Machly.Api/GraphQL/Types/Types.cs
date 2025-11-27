using Machly.Api.GraphQL.DTOs;
using HotChocolate.Types;

namespace Machly.Api.GraphQL.Types
{
    public class MachineType : ObjectType<MachineDto>
    {
        protected override void Configure(IObjectTypeDescriptor<MachineDto> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Field(m => m.Id).Type<NonNullType<StringType>>();
            descriptor.Field(m => m.ProviderId).Type<NonNullType<StringType>>();
            descriptor.Field(m => m.Title).Type<NonNullType<StringType>>();
            descriptor.Field(m => m.Description).Type<NonNullType<StringType>>();
            descriptor.Field(m => m.PricePerDay).Type<NonNullType<DecimalType>>();
            descriptor.Field(m => m.Category).Type<NonNullType<StringType>>();
            descriptor.Field(m => m.Type).Type<NonNullType<StringType>>();
            descriptor.Field(m => m.Lat).Type<NonNullType<FloatType>>();
            descriptor.Field(m => m.Lng).Type<NonNullType<FloatType>>();
            descriptor.Field(m => m.Photos).Type<ListType<StringType>>();
            descriptor.Field(m => m.IsOutOfService).Type<NonNullType<BooleanType>>();
            descriptor.Field(m => m.RatingAvg).Type<NonNullType<FloatType>>();
            descriptor.Field(m => m.RatingCount).Type<NonNullType<IntType>>();
        }
    }

    public class BookingType : ObjectType<BookingDto>
    {
        protected override void Configure(IObjectTypeDescriptor<BookingDto> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Field(b => b.Id).Type<NonNullType<StringType>>();
            descriptor.Field(b => b.MachineId).Type<NonNullType<StringType>>();
            descriptor.Field(b => b.RenterId).Type<NonNullType<StringType>>();
            descriptor.Field(b => b.Start).Type<NonNullType<DateTimeType>>();
            descriptor.Field(b => b.End).Type<NonNullType<DateTimeType>>();
            descriptor.Field(b => b.TotalPrice).Type<NonNullType<DecimalType>>();
            descriptor.Field(b => b.Status).Type<NonNullType<StringType>>();
            descriptor.Field(b => b.CheckInDate).Type<DateTimeType>();
            descriptor.Field(b => b.CheckOutDate).Type<DateTimeType>();
            descriptor.Field(b => b.ReviewRating).Type<IntType>();
            descriptor.Field(b => b.ReviewComment).Type<StringType>();
        }
    }

    public class UserType : ObjectType<UserDto>
    {
        protected override void Configure(IObjectTypeDescriptor<UserDto> descriptor)
        {
            descriptor.BindFieldsExplicitly();

            descriptor.Field(u => u.Id).Type<NonNullType<StringType>>();
            descriptor.Field(u => u.Name).Type<NonNullType<StringType>>();
            descriptor.Field(u => u.Email).Type<NonNullType<StringType>>();
            descriptor.Field(u => u.Role).Type<NonNullType<StringType>>();
            descriptor.Field(u => u.PhotoUrl).Type<StringType>();
        }
    }
}
