# 🏢 Project Defense – Room booking for students

**Project Defense** is a modern web-based conference room booking system built with **ASP.NET Core (MVC with Razor Pages)**, **PostgreSQL**, and **Minimal API**.  
It enables **students** to reserve meeting slots with **instructors**, who can manage their availability, bookings, and exceptions (e.g., illness or blocked periods).  
The app ensures **secure authentication**, **role-based access**, **bilingual support**, and includes a **REST API** documented with **Swagger**.

---

## 🚀 Features

### 🔐 User Authentication & Roles
- Secure registration and login with **email confirmation**
- Built on **ASP.NET Core Identity**
- Separate user roles:
  - **Student**
  - **Instructor**

---

### 🏫 Room & Slot Management
- Instructors define available time slots for specific rooms  
- System prevents **overlapping time slots** for the same room/time

---

### 📅 Booking Functionality
- Students can **view and reserve** available slots  
- Intuitive **calendar-based navigation** for selecting times

---

### 🧑‍🏫 Instructor Panel
- Instructors can:
  - View all reservations
  - Cancel or transfer bookings
  - Block off unavailable periods  
- System automatically **disables expired or invalid slots**

---

### 🌐 Internationalization
- Localized **date/time formats**

---

### ⚙️ Minimal API + Swagger
- RESTful API for:
  - Retrieving available slots and rooms
  - Making reservations  
- **Swagger UI** integration for easy testing and documentation

---

### 💻 Console Client
- Simple **.NET Core console app** for interacting with the API  
- Fetch and book slots directly from the terminal

---

### 🐳 Docker Support
- Pre-configured **Dockerfile** and **docker-compose** for quick setup  
- Launch web server and PostgreSQL database together

---

### ✅ Rich Validation
- Server-side and client-side form validation  
- Enforces business rules automatically

---

### 📧 Email Notifications
- Integrated **SendGrid** support for sending email alerts on key actions

---

## 🧱 Technology Stack

| Component | Technology |
|------------|-------------|
| **Backend** | ASP.NET Core 8/9 (MVC + Minimal APIs), Entity Framework Core |
| **Frontend** | Razor Pages / Views, Bootstrap 5 |
| **Database** | PostgreSQL |
| **API Docs** | Swagger / OpenAPI |
| **Email** | SendGrid API |
| **Localization** | .NET Core localization middleware |
| **Deployment** | Docker / Docker Compose |

---
