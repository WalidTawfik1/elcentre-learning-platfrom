# ElCentre Learning Platform

A comprehensive online learning management system built with ASP.NET Core and modern web technologies. ElCentre provides a complete educational ecosystem for students, instructors, and administrators.

## 🌟 Features Overview

### 👤 User Management & Authentication
- **Multi-Role System**: Students, Instructors, and Administrators
- **Secure Authentication**: JWT token-based authentication with ASP.NET Core Identity
- **Social Login**: Google OAuth integration with role selection
- **Account Security**:
  - Email verification with OTP (One-Time Password) system
  - Password reset functionality
  - Account activation via email verification
  - Profile management with picture uploads

### 📚 Course Management
- **Course Creation & Management**:
  - Rich course content with titles, descriptions, and thumbnails
  - Course categorization system
  - Pricing and duration management
  - Course status tracking (Pending, Approved, Active)
  - Instructor-specific course management
  - Course approval workflow for administrators

- **Course Structure**:
  - Modular course organization
  - Hierarchical content structure (Course → Modules → Lessons)
  - Flexible content ordering system

### 🎓 Learning Experience
- **Interactive Lessons**:
  - Multiple content types support
  - Video lessons with Cloudinary integration
  - Text-based content
  - Lesson completion tracking
  - Progress monitoring

- **Quiz System**:
  - Course-specific quizzes
  - Lesson-based assessments
  - Multiple choice questions (A, B, C, D options)
  - Automatic scoring system
  - Detailed explanations for answers
  - Student quiz attempt tracking

### 📊 Progress Tracking & Analytics
- **Student Progress**:
  - Lesson completion status
  - Course progress percentage calculation
  - Quiz performance tracking
  - Enrollment status monitoring

- **Instructor Analytics**:
  - Student enrollment statistics
  - Course performance metrics
  - Progress tracking for all enrolled students

### 💳 Payment & Enrollment System
- **Secure Payments**:
  - Paymob payment gateway integration
  - Multiple payment methods (Card, Wallet)
  - Payment status tracking
  - Enrollment management

- **Enrollment Features**:
  - One-click course enrollment
  - Enrollment status management
  - Student-course relationship tracking

### 🔔 Real-time Notifications
- **SignalR Integration**:
  - Real-time notifications for students
  - Course-specific notification groups
  - Instructor-to-student communication
  - New lesson announcements
  - Automatic notification system

### ⭐ Review & Rating System
- **Course Reviews**:
  - Student feedback collection
  - Rating system for courses
  - Review management
  - Instructor performance insights

### 🎨 Media Management
- **Cloudinary Integration**:
  - Course thumbnail management
  - Profile picture uploads
  - Video content hosting
  - Optimized media delivery

### 🏗️ Technical Architecture

#### 🔧 Backend Technologies
- **Framework**: ASP.NET Core 8.0
- **Authentication**: ASP.NET Core Identity with JWT
- **Database**: Entity Framework Core with SQL Server
- **Real-time**: SignalR for live notifications
- **Logging**: Serilog for comprehensive logging
- **Documentation**: Swagger/OpenAPI integration
- **Mapping**: AutoMapper for object-to-object mapping

#### 🏛️ Architecture Pattern
- **Clean Architecture**: Separation of concerns with distinct layers
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: Built-in ASP.NET Core DI container

#### 📁 Project Structure
```
ElCentre.sln
├── ElCentre.API/          # Web API layer
├── ElCentre.Core/         # Domain entities and DTOs
├── ElCentre.Infrastructure/ # Data access and external services
└── ElCentre.Contracts/    # SignalR hubs and contracts
```

### 🔐 Security Features
- **Role-based Authorization**: Fine-grained access control
- **JWT Token Authentication**: Secure API access
- **CORS Configuration**: Cross-origin request handling
- **Input Validation**: Data transfer object validation
- **Exception Handling**: Global exception middleware

### 📱 API Features
- **RESTful Design**: Clean and intuitive API endpoints
- **Comprehensive Documentation**: Swagger UI integration
- **Error Handling**: Standardized API responses
- **Pagination Support**: Efficient data retrieval
- **File Upload Support**: Multi-part form data handling

## 📝 License

This project is licensed under **All Rights Reserved**.  
Unauthorized use or distribution is prohibited. - see the [LICENSE](LICENSE) file for details.


## 📞 Support

For support and questions, please open an issue in the GitHub repository or contact me.

---

**ElCentre Learning Platform** - Empowering education through technology 🎓✨
