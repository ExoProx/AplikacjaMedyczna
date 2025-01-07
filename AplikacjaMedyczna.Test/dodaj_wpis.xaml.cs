using Microsoft.VisualStudio.TestTools.UnitTesting;
using AplikacjaMedyczna;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace AplikacjaMedyczna.Tests
{
    [TestClass]
    public class DodajWpisTests
    {
        private dodaj_wpis _dodajWpis;

        [TestInitialize]
        public void Setup()
        {
            _dodajWpis = new dodaj_wpis();
            Assert.IsNotNull(_dodajWpis, "Obiekt dodaj_wpis nie zosta³ poprawnie zainicjalizowany.");
        }

        [UITestMethod]
        public void SaveEntry_EmptyField_ReturnsZero()
        {
            // Arrange
            string wpis = "";

            // Act
            int result = _dodajWpis.SaveEntry(wpis);

            // Assert
            Assert.AreEqual(0, result);
        }

        [UITestMethod]
        public void SaveEntry_ValidEntry_ReturnsOne()
        {
            // Arrange
            string wpis = "Testowy wpis";
            SharedData.pesel = "12345678901";
            SharedData.id = "1";

            // Act
            int result = _dodajWpis.SaveEntry(wpis);

            // Assert
            Assert.AreEqual(1, result);
        }

        [UITestMethod]
        public void SaveEntry_DatabaseError_ReturnsThree()
        {
            // Arrange
            string wpis = "Testowy wpis";
            SharedData.pesel = "12345678901";
            SharedData.id = "1";

            // Simulate database error by providing incorrect connection string
            var originalConnectionString = "host=invalid_host;username=invalid_user;Password=invalid_password;Database=invalid_database";
            var field = typeof(dodaj_wpis).GetField("connectionString", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_dodajWpis, originalConnectionString);

            // Act
            int result = _dodajWpis.SaveEntry(wpis);

            // Assert
            Assert.AreEqual(3, result);
        }
    }
}