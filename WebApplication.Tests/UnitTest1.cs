using Xunit;

namespace WebApplication.Tests
{
    // ═══════════════════════════════════════════════════════════
    // WHY UNIT TESTS?
    // Unit tests verify individual pieces of logic in isolation.
    // They run without a database or browser — just pure C# math
    // and logic checks. Each [Fact] is one test case.
    // ═══════════════════════════════════════════════════════════

    // ───────────────────────────────────────────────────────────
    // 1. CURRENCY CALCULATION TESTS
    //    Requirement: Verify USD → ZAR conversion math is correct
    // ───────────────────────────────────────────────────────────
    public class CurrencyCalculationTests
    {
        // This is the same formula used in the app.
        // We test it here in isolation — no HTTP call needed.
        private decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
        {
            return Math.Round(usdAmount * rate, 2);
        }

        [Fact]
        public void Convert_100USD_At18_50Rate_Returns1850()
        {
            // Arrange
            decimal usd = 100m;
            decimal rate = 18.50m;

            // Act
            decimal result = ConvertUsdToZar(usd, rate);

            // Assert
            Assert.Equal(1850.00m, result);
        }

        [Fact]
        public void Convert_500USD_At19_25Rate_Returns9625()
        {
            decimal result = ConvertUsdToZar(500m, 19.25m);
            Assert.Equal(9625.00m, result);
        }

        [Fact]
        public void Convert_0USD_ReturnsZero()
        {
            decimal result = ConvertUsdToZar(0m, 18.50m);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void Convert_RoundsToTwoDecimalPlaces()
        {
            // 1 USD at 18.123456 should round to 18.12
            decimal result = ConvertUsdToZar(1m, 18.123456m);
            Assert.Equal(18.12m, result);
        }

        [Fact]
        public void Convert_LargeAmount_IsCorrect()
        {
            // 10,000 USD at 18.50 = 185,000.00 ZAR
            decimal result = ConvertUsdToZar(10000m, 18.50m);
            Assert.Equal(185000.00m, result);
        }
    }

    // ───────────────────────────────────────────────────────────
    // 2. FILE VALIDATION TESTS
    //    Requirement: Only .pdf allowed — .exe and others rejected
    // ───────────────────────────────────────────────────────────
    public class FileValidationTests
    {
        // This mirrors the exact validation logic in ContractsController
        private bool IsPdfFile(string fileName)
        {
            return Path.GetExtension(fileName).ToLower() == ".pdf";
        }

        [Fact]
        public void PdfFile_IsAllowed()
        {
            Assert.True(IsPdfFile("agreement.pdf"));
        }

        [Fact]
        public void PdfUppercase_IsAllowed()
        {
            // Must be case-insensitive
            Assert.True(IsPdfFile("CONTRACT.PDF"));
        }

        [Fact]
        public void ExeFile_IsRejected()
        {
            Assert.False(IsPdfFile("malware.exe"));
        }

        [Fact]
        public void DocxFile_IsRejected()
        {
            Assert.False(IsPdfFile("contract.docx"));
        }

        [Fact]
        public void ExcelFile_IsRejected()
        {
            Assert.False(IsPdfFile("spreadsheet.xlsx"));
        }

        [Fact]
        public void JpgFile_IsRejected()
        {
            Assert.False(IsPdfFile("scan.jpg"));
        }

        [Fact]
        public void NoExtension_IsRejected()
        {
            Assert.False(IsPdfFile("filename_without_extension"));
        }
    }

    // ───────────────────────────────────────────────────────────
    // 3. WORKFLOW ENFORCEMENT TESTS
    //    Requirement: ServiceRequest cannot be created if Contract
    //    is Expired or On Hold
    // ───────────────────────────────────────────────────────────
    public class WorkflowEnforcementTests
    {
        // Same logic used in ServiceRequestsController
        private bool CanCreateServiceRequest(string contractStatus)
        {
            return contractStatus != "Expired" && contractStatus != "On Hold";
        }

        [Fact]
        public void ExpiredContract_BlocksServiceRequest()
        {
            Assert.False(CanCreateServiceRequest("Expired"));
        }

        [Fact]
        public void OnHoldContract_BlocksServiceRequest()
        {
            Assert.False(CanCreateServiceRequest("On Hold"));
        }

        [Fact]
        public void ActiveContract_AllowsServiceRequest()
        {
            Assert.True(CanCreateServiceRequest("Active"));
        }

        [Fact]
        public void DraftContract_AllowsServiceRequest()
        {
            Assert.True(CanCreateServiceRequest("Draft"));
        }
    }

    // ───────────────────────────────────────────────────────────
    // 4. CONTRACT DATE VALIDATION TESTS
    //    End date must be after start date
    // ───────────────────────────────────────────────────────────
    public class ContractDateValidationTests
    {
        private bool IsValidDateRange(DateTime start, DateTime end)
        {
            return end > start;
        }

        [Fact]
        public void EndDateAfterStartDate_IsValid()
        {
            Assert.True(IsValidDateRange(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 12, 31)));
        }

        [Fact]
        public void EndDateBeforeStartDate_IsInvalid()
        {
            Assert.False(IsValidDateRange(
                new DateTime(2024, 12, 31),
                new DateTime(2024, 1, 1)));
        }

        [Fact]
        public void SameDates_IsInvalid()
        {
            var date = new DateTime(2024, 6, 1);
            Assert.False(IsValidDateRange(date, date));
        }
    }
}