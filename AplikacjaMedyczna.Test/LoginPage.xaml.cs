using Microsoft.VisualStudio.TestTools.UnitTesting;
using AplikacjaMedyczna;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace AplikacjaMedyczna.Tests
{
    [TestClass]
    public class LoginPageTests
    {
        [UITestMethod]
        public void Logowanie_InvalidPeselLength_Returns0()
        {
            // Arrange
            var loginPage = new LoginPage();
            string pesel = "123456789"; // Invalid length
            string haslo = "password";

            // Act
            int result = loginPage.logowanie(pesel, haslo);

            // Assert
            Assert.AreEqual(0, result);
        }

        [UITestMethod]
        public void Logowanie_InvalidPeselFormat_Returns0()
        {
            // Arrange
            var loginPage = new LoginPage();
            string pesel = "abcdefghijk"; // Invalid format
            string haslo = "password";

            // Act
            int result = loginPage.logowanie(pesel, haslo);

            // Assert
            Assert.AreEqual(0, result);
        }

        [UITestMethod]
        public void Logowanie_ValidPeselInvalidPassword_Returns2()
        {
            // Arrange
            var loginPage = new LoginPage();
            string pesel = "22222222222"; // Valid PESEL
            string haslo = "wrongpassword";

            // Act
            int result = loginPage.logowanie(pesel, haslo);

            // Assert
            Assert.AreEqual(2, result);
        }

        [UITestMethod]
        public void Logowanie_ValidPeselValidPassword_Returns1()
        {
            // Arrange
            var loginPage = new LoginPage();
            string pesel = "22222222222"; // Valid PESEL
            string haslo = "haslo";

            // Act
            int result = loginPage.logowanie(pesel, haslo);

            // Assert
            Assert.AreEqual(1, result);
        }

        [UITestMethod]
        public void Logowanie_InvalidPesel_Returns3()
        {
            // Arrange
            var loginPage = new LoginPage();
            string pesel = "00000000000"; // Invalid PESEL
            string haslo = "password";

            // Act
            int result = loginPage.logowanie(pesel, haslo);

            // Assert
            Assert.AreEqual(3, result);
        }
    }
}