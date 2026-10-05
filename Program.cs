
// Mã sinh viên: 202418922
// Họ tên: Phạm Nam Kha

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab04
{
    // ========================================================
    // 1. CẤU TRÚC LƯU LỊCH SỬ THƯỞNG (QUYẾT ĐỊNH THIẾT KẾ A.5)
    // ========================================================
    public class BonusRecord
    {
        public double Amount { get; }
        public string Reason { get; }

        public BonusRecord(double amount, string reason)
        {
            Amount = amount;
            Reason = reason;
        }
    }

    // ========================================================
    // 2. LỚP TRỪU TƯỢNG EMPLOYEE (LỚP CƠ SỞ)
    // ========================================================
    public abstract class Employee
    {
        public string EmployeeId { get; protected set; }
        public string FullName { get; protected set; }
        public string Department { get; protected set; }
        
        // Quản lý lịch sử thưởng thay vì chỉ lưu 1 biến cộng dồn
        protected List<BonusRecord> BonusHistory { get; } = new List<BonusRecord>();

        public double MonthlyBonus => BonusHistory.Sum(b => b.Amount);

        // Constructor nạp chồng
        public Employee(string employeeId, string fullName) 
            : this(employeeId, fullName, "Unassigned") { }

        public Employee(string employeeId, string fullName, string department)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Mã nhân sự không được để trống.");
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Họ tên không được để trống.");
            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Phòng ban không được để trống.");

            EmployeeId = employeeId;
            FullName = fullName;
            Department = department;
        }

        // Nạp chồng phương thức addBonus
        public void AddBonus(double amount)
        {
            AddBonus(amount, "Khen thưởng định kỳ");
        }

        public void AddBonus(double amount, string reason)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền thưởng phải lớn hơn 0.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Lý do thưởng không được để trống.");

            BonusHistory.Add(new BonusRecord(amount, reason));
        }

        public void AddBonus(double rate, double referenceAmount, string reason)
        {
            if (rate <= 0 || rate > 0.5)
                throw new ArgumentException("Tỷ lệ thưởng phải nằm trong khoảng (0, 0.5].");
            if (referenceAmount <= 0)
                throw new ArgumentException("Giá trị tham chiếu phải lớn hơn 0.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Lý do thưởng không được để trống.");

            double bonusAmount = rate * referenceAmount;
            BonusHistory.Add(new BonusRecord(bonusAmount, reason));
        }

        public void ResetMonthlyBonus()
        {
            BonusHistory.Clear();
        }

        // Các hành vi trừu tượng và ảo
        public abstract double CalculateGrossPay();
        public abstract string GetEmployeeType();

        public virtual void DisplayPayrollInfo()
        {
            Console.WriteLine($"[{GetEmployeeType()}] Mã: {EmployeeId} | Tên: {FullName} | Phòng: {Department} | Thưởng: {MonthlyBonus:N0} VNĐ | Thực nhận: {CalculateGrossPay():N0} VNĐ");
        }
    }

    // ========================================================
    // 3. LỚP SALARIED EMPLOYEE (NHÂN VIÊN LƯƠNG CỐ ĐỊNH)
    // ========================================================
    public class SalariedEmployee : Employee
    {
        public double MonthlySalary { get; private set; }
        public double ResponsibilityAllowance { get; private set; }

        public SalariedEmployee(string id, string name, double salary)
            : this(id, name, "Unassigned", salary, 0) { }

        public SalariedEmployee(string id, string name, string dept, double salary, double allowance)
            : base(id, name, dept)
        {
            if (salary < 0) throw new ArgumentException("Lương cố định không được âm.");
            if (allowance < 0) throw new ArgumentException("Phụ cấp trách nhiệm không được âm.");

            MonthlySalary = salary;
            ResponsibilityAllowance = allowance;
        }

        public override double CalculateGrossPay()
        {
            return MonthlySalary + ResponsibilityAllowance + MonthlyBonus;
        }

        public override string GetEmployeeType() => "Salaried";

        public override void DisplayPayrollInfo()
        {
            Console.WriteLine($"[{GetEmployeeType()}] {EmployeeId} - {FullName} ({Department}) | Lương: {MonthlySalary:N0} | Phụ cấp: {ResponsibilityAllowance:N0} | Thưởng: {MonthlyBonus:N0} => Tổng: {CalculateGrossPay():N0} VNĐ");
        }
    }

    // ========================================================
    // 4. LỚP HOURLY EMPLOYEE (NHÂN VIÊN THEO GIỜ)
    // ========================================================
    public class HourlyEmployee : Employee
    {
        public double HourlyRate { get; private set; }
        public double WorkedHours { get; private set; }

        public HourlyEmployee(string id, string name, double rate)
            : this(id, name, "Unassigned", rate, 0) { }

        public HourlyEmployee(string id, string name, string dept, double rate, double hours)
            : base(id, name, dept)
        {
            if (rate < 0) throw new ArgumentException("Đơn giá giờ không được âm.");
            if (hours < 0 || hours > 250) throw new ArgumentException("Số giờ làm phải từ 0 đến 250 giờ.");

            HourlyRate = rate;
            WorkedHours = hours;
        }

        public override double CalculateGrossPay()
        {
            double basePay;
            if (WorkedHours <= 160)
            {
                basePay = WorkedHours * HourlyRate;
            }
            else
            {
                basePay = (160 * HourlyRate) + ((WorkedHours - 160) * HourlyRate * 1.5);
            }
            return basePay + MonthlyBonus;
        }

        public override string GetEmployeeType() => "Hourly";

        public override void DisplayPayrollInfo()
        {
            double otHours = WorkedHours > 160 ? WorkedHours - 160 : 0;
            Console.WriteLine($"[{GetEmployeeType()}] {EmployeeId} - {FullName} ({Department}) | Giờ làm: {WorkedHours} (OT: {otHours}) | Đơn giá: {HourlyRate:N0} | Thưởng: {MonthlyBonus:N0} => Tổng: {CalculateGrossPay():N0} VNĐ");
        }
    }

    // ========================================================
    // 5. LỚP SALES EMPLOYEE (NHÂN VIÊN KINH DOANH)
    // ========================================================
    public class SalesEmployee : Employee
    {
        public double BaseSalary { get; private set; }
        public double SalesRevenue { get; private set; }
        public double CommissionRate { get; private set; }

        public SalesEmployee(string id, string name, double baseSalary)
            : this(id, name, "Unassigned", baseSalary, 0, 0) { }

        public SalesEmployee(string id, string name, string dept, double baseSalary, double revenue, double rate)
            : base(id, name, dept)
        {
            if (baseSalary < 0) throw new ArgumentException("Lương cơ bản không được âm.");
            if (revenue < 0) throw new ArgumentException("Doanh số không được âm.");
            if (rate < 0 || rate > 0.3) throw new ArgumentException("Tỷ lệ hoa hồng phải từ 0 đến 0.3 (30%).");

            BaseSalary = baseSalary;
            SalesRevenue = revenue;
            CommissionRate = rate;
        }

        public void UpdateSalesRevenue(double revenue)
        {
            if (revenue < 0) throw new ArgumentException("Doanh số không được âm.");
            SalesRevenue = revenue;
        }

        public override double CalculateGrossPay()
        {
            return BaseSalary + (SalesRevenue * CommissionRate) + MonthlyBonus;
        }

        public override string GetEmployeeType() => "Sales";

        public override void DisplayPayrollInfo()
        {
            Console.WriteLine($"[{GetEmployeeType()}] {EmployeeId} - {FullName} ({Department}) | Lương cứng: {BaseSalary:N0} | Doanh số: {SalesRevenue:N0} | Hoa hồng: {CommissionRate * 100}% | Thưởng: {MonthlyBonus:N0} => Tổng: {CalculateGrossPay():N0} VNĐ");
        }
    }

    // ========================================================
    // 6. LỚP PAYROLL (TỔNG HỢP VÀ QUẢN LÝ BẢNG LƯƠNG)
    // ========================================================
    public class Payroll
    {
        public string Period { get; }
        private readonly List<Employee> employees = new List<Employee>();

        public Payroll(string period)
        {
            if (string.IsNullOrWhiteSpace(period))
                throw new ArgumentException("Kỳ lương không được để trống.");
            Period = period;
        }

        public bool AddEmployee(Employee employee)
        {
            if (employee == null) return false;
            if (employees.Any(e => e.EmployeeId == employee.EmployeeId))
            {
                Console.WriteLine($"[Lỗi Thêm] Nhân sự mã '{employee.EmployeeId}' đã tồn tại trong kỳ {Period}.");
                return false;
            }
            employees.Add(employee);
            return true;
        }

        public Employee FindEmployee(string employeeId)
        {
            return employees.FirstOrDefault(e => e.EmployeeId == employeeId);
        }

        public double CalculateTotalPayroll()
        {
            // Gọi calculateGrossPay() đa hình qua lớp cơ sở Employee
            return employees.Sum(e => e.CalculateGrossPay());
        }

        public double CalculatePayrollByDepartment(string department)
        {
            return employees
                .Where(e => e.Department.Equals(department, StringComparison.OrdinalIgnoreCase))
                .Sum(e => e.CalculateGrossPay());
        }

        public Employee FindHighestPaidEmployee()
        {
            if (!employees.Any()) return null;
            return employees.OrderByDescending(e => e.CalculateGrossPay()).FirstOrDefault();
        }

        public void DisplayPayroll()
        {
            Console.WriteLine($"\n========================================================");
            Console.WriteLine($"           BẢNG LƯƠNG DOANH NGHIỆP - KỲ {Period}");
            Console.WriteLine($"========================================================");
            if (!employees.Any())
            {
                Console.WriteLine("Danh sách nhân sự kỳ này đang trống.");
                return;
            }

            foreach (var emp in employees)
            {
                emp.DisplayPayrollInfo();
            }

            Console.WriteLine("--------------------------------------------------------");
            Console.WriteLine($"TỔNG CHI PHÍ BẢNG LƯƠNG: {CalculateTotalPayroll():N0} VNĐ");
            Console.WriteLine($"TỔNG LƯƠNG PHÒNG 'Hỗ trợ': {CalculatePayrollByDepartment("Hỗ trợ"):N0} VNĐ");
            
            var topEmp = FindHighestPaidEmployee();
            if (topEmp != null)
            {
                Console.WriteLine($"NHÂN VIÊN CÓ THU NHẬP CAO NHẤT: {topEmp.FullName} ({topEmp.CalculateGrossPay():N0} VNĐ)");
            }
            Console.WriteLine($"========================================================\n");
        }
    }

    // ========================================================
    // 7. CHƯƠNG TRÌNH KIỂM THỬ TỔNG HỢP VÀ TRƯỜNG HỢP BIÊN
    // ========================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== BẮT ĐẦU KIỂM THỬ CHUẨN DỮ LIỆU ĐỀ BÀI (MỤC C) ===");
            Payroll payroll = new Payroll("2026-09");

            // 1. Tạo E001 (Salaried)
            var e1 = new SalariedEmployee("E001", "Nguyễn Minh An", "Đào tạo", 15000000, 2000000);
            e1.AddBonus(1000000, "Thưởng hoàn thành khóa học");
            payroll.AddEmployee(e1);

            // 2. Tạo E002 (Hourly không OT)
            var e2 = new HourlyEmployee("E002", "Trần Thu Bình", "Hỗ trợ", 100000, 150);
            e2.AddBonus(500000);
            payroll.AddEmployee(e2);

            // 3. Tạo E003 (Hourly có OT)
            var e3 = new HourlyEmployee("E003", "Lê Hoàng Chi", "Hỗ trợ", 100000, 170);
            payroll.AddEmployee(e3);

            // 4. Tạo E004 (Sales)
            var e4 = new SalesEmployee("E004", "Phạm Quốc Dũng", "Kinh doanh", 8000000, 200000000, 0.05);
            e4.AddBonus(0.02, 50000000, "Thưởng vượt KPI tháng");
            payroll.AddEmployee(e4);

            // Hiển thị bảng lương chuẩn
            payroll.DisplayPayroll();

            Console.WriteLine("\n=== KIỂM THỬ 10 TÌNH HUỐNG BIÊN & BẮT LỖI (MỤC C.1) ===");

            TestInvariant("Test 1: Mã nhân viên để trống", () => new SalariedEmployee("", "Nguyen Van X", 10000000));
            TestInvariant("Test 2: Lương cố định âm", () => new SalariedEmployee("E005", "Nguyen Van X", -5000000));
            TestInvariant("Test 3: Số giờ làm vượt quá 250 giờ", () => new HourlyEmployee("E006", "Nguyen Van Y", "Unassigned", 100000, 260));
            TestInvariant("Test 4: Số giờ làm âm", () => new HourlyEmployee("E007", "Nguyen Van Y", "Unassigned", 100000, -5));
            TestInvariant("Test 5: Tỷ lệ hoa hồng vượt quá 0.3 (0.35)", () => new SalesEmployee("E008", "Tran Van Z", "Unassigned", 5000000, 100000000, 0.35));~
            TestInvariant("Test 6: Thưởng cố định âm", () => e1.AddBonus(-200000));
            TestInvariant("Test 7: Tỷ lệ thưởng tham chiếu vượt 0.5 (0.6)", () => e1.AddBonus(0.6, 10000000, "Thuong"));
            TestInvariant("Test 8: Lý do thưởng để trống", () => e1.AddBonus(100000, "   "));
            TestInvariant("Test 9: Thêm nhân viên trùng mã E001 vào Payroll", () => {
                var duplicate = new SalariedEmployee("E001", "Trùng Mã", 10000000);
                if (!payroll.AddEmployee(duplicate)) throw new InvalidOperationException("Bị chặn trùng mã thành công.");
            });
            TestInvariant("Test 10: Xử lý Payroll rỗng", () => {
                Payroll emptyPayroll = new Payroll("2026-10");
                if (emptyPayroll.CalculateTotalPayroll() != 0 || emptyPayroll.FindHighestPaidEmployee() != null)
                    throw new Exception("Lỗi xử lý bảng lương rỗng.");
                Console.WriteLine(" -> Xử lý bảng lương rỗng hoàn toàn chính xác.");
            });

            Console.WriteLine("\nHoàn tất toàn bộ kịch bản kiểm thử!");
        }

        static void TestInvariant(string testName, Action action)
        {
            try
            {
                action();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[PASS/NO EXCEPTION] {testName}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[BẮT NGOẠI LỆ THÀNH CÔNG] {testName} => Lý do: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}

