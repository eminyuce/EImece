# 🛍️ EImece - ECommerce Web Application

**EImece** is an open-source, full-featured eCommerce web application built with **ASP.NET MVC 5**, **Entity Framework 6**, and **Ninject**. It follows a clean architecture using the **Repository Pattern** and **Service Layer** for better scalability and maintainability.

---

## 🚀 Technologies Used

- **C#** – Main programming language  
- **ASP.NET MVC 5.2.3** – Web application framework  
- **Entity Framework 6** – ORM for data access  
- **SQL Server 2008** – Database system  
- **Ninject** – Dependency injection  
- **jQuery** – JavaScript library for dynamic UI  
- **Bootstrap** – Responsive front-end design  
- **Generic Repository Pattern** – Abstraction for data layer  
- **Service Layer Pattern** – Separation of business logic  

---

## 💎 Highlight Feature: Iyzico Payment Integration

### ✅ Seamless Payment Experience with [Iyzico](https://www.iyzico.com/en)
- Fully integrated with **Iyzico** for real-time, secure payment processing
- Supports **guest** and **registered user** checkouts
- Customers receive payment confirmation and **tracking numbers**
- Iyzico provides **PCI-DSS compliant** infrastructure for safe transactions

> This makes **EImece** production-ready for real-world commercial use in Turkey and beyond!

---

## 🎯 Key Features

### 🏠 Home Page
- Scrolling image banners with link support
- Custom menu items and themed content pages

### 🛒 Product Management
- Product categorization and tagging
- Image gallery support for each product
- Price updates by category, tag, or brand
- Product filtering by price, category, rating, and brand

### 💳 Shopping & Payment
- Add to cart functionality
- **Secure payments via Iyzico**
- Guest or member checkout options
- Order tracking with cargo numbers
- Fraud prevention and secure transaction handling

### 👤 Customer & Order Management
- Admin dashboard to manage customers and orders
- Add cargo numbers, update status, and add internal notes
- Display FAQs for each product on customer account page

### 📞 Communication & Social Integration
- Contact form with **email** and **WhatsApp** support

---

## 📁 Project Structure
Uses Repository + Service Layer for clean separation:
```
Controllers/
Models/
Services/
Repositories/
Views/
```

---

## 🧑‍💻 Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/eminyuce/EImece.git
   ```
2. Open the solution in **Visual Studio**.
3. Configure the database connection **without committing secrets** — set `EIMECE_DB_CONNECTION_STRING` or use a gitignored `ConnectionStrings.config`. See [EImece/docs/SECURE_CONNECTION_STRINGS.md](EImece/docs/SECURE_CONNECTION_STRINGS.md).
4. Run the application and apply migrations if needed.

---

## 🤝 Contributions
Have ideas to improve the platform? Fork the project, create a new branch, and submit a pull request!

---

## 📜 License
Licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

 
