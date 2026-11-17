using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ReceiptSnapshotTests
    {
        [Fact]
        public async Task ReceiptSnapshot_ShouldMatchExpected()
        {
            //arrange
            var repositoryMock = new Mock<IOrderRepository>();
            var inventoryMock = new Mock<IInventoryService>();
            var notificationMock = new Mock<INotificationService>();

            inventoryMock.Setup(x=>x.IsAvailableAsync(It.IsAny<Guid>(),It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var checkoutService =new CheckoutService(repositoryMock.Object, inventoryMock.Object, notificationMock.Object);

            var order=new Order
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CustomerName = "John Doe",
                CustomerEmail = "john.doe@example.com",
                Items=new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        ProductName = "LapTop",
                        Quantity = 3,
                        UnitPrice = 500m
                    },
                  
                }
            };

            // Act
            var receipt = await checkoutService.CheckoutAsync(order);

            // Assert
            receipt.Subtotal.Should().Be(1500m);
            receipt.Discount.Should().Be(100m);
            receipt.Total.Should().Be(1400m);


            var setting = new VerifySettings();
            setting.IgnoreMember<CheckoutReceipt>(o => o.CreatedAt);

           await Verifier.Verify(receipt, setting);

        }
    }
}
