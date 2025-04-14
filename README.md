# 🛍️ EImece - ECommerce Web Application

**EImece** is an open-source, feature-rich eCommerce web application built using **ASP.NET MVC 5**, **Entity Framework 6**, and **Ninject**. Designed with a layered architecture utilizing the **Repository Pattern** and **Service Layer**, this application provides a scalable and maintainable solution for online retail platforms.

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
- **Iyzico Integration for Payment** – Payment over Iyzico for any purchase  
---

## 🎯 Key Features

### 🏠 Home Page
- Dynamic image carousel with clickable links
- Menu-based navigation to themed content pages

### 🛒 Product Management
- Categorize and tag products
- Upload main and gallery images for each product
- Bulk price updates by category, tag, or brand
- Advanced product filtering: price, category, rating, brand

### 💳 Shopping & Payment
- Add to cart and checkout using **iyzico** payment gateway
- Supports both guest and registered user checkouts
- Customers receive a tracking number for shipping
- Secure and reliable payment integration

### 👤 Customer & Order Management
- Admin panel for managing users and orders
- Order tracking, status updates, comment handling
- Product-specific FAQs visible in the customer account

### 📞 Communication & Social Integration
- Contact via **email** or **WhatsApp** through the contact page

---

## 📁 Project Structure
The application is structured using the **Repository** and **Service Layer** pattern:
```
Controllers/
Models/
Services/
Repositories/
Views/
```
This promotes separation of concerns and makes the application easy to test, extend, and maintain.

---

## 🧑‍💻 Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/eiemce-ecommerce.git
   ```
2. Open the solution in **Visual Studio**.
3. Update the database connection string in `Web.config`.
4. Run the application and seed the database if needed.

---

## 🤝 Contributions
Contributions are welcome! Feel free to fork the project and submit pull requests. Open an issue if you encounter bugs or have feature requests.

---

## 📜 License
This project is open-sourced under the MIT License. See the [LICENSE](LICENSE) file for details.
