### Legacy System Modernization & Inventory Management (Reservation/Pickup System for Clerks/Cashier)

## Legacy System Modernization & Inventory Management

A ground-up modernization of a legacy WinForms VB.NET application, independently re-architected as an ASP.NET Core MVC web application to serve as the operational backbone for inventory management and reservation fulfillment of Estaleiro Brasfels.

##The Challenge
Faced with an outdated, undocumented WinForms VB.NET system that lacked proper tracking for reservation/pickup operations. As the sole developer on this modernization effort, I had to reverse-engineer the existing system without documentation or guidance to understand business logic and data relationships.


## The Solution
I independently analyzed the legacy codebase and designed a modern ASP.NET Core MVC replacement, with primary focus on implementing comprehensive audit trails and reporting specifically for the reservation and pickup lifecycle. This transformed an opaque process into a fully traceable operational workflow.


## Key Improvements Delivered
1.) End-to-End Reservation Tracking: Complete audit trail from team leader reservation to clerk fulfillment and item handoff

2.) Pickup Process Management: Streamlined interface for clerks to process reservations, print tickets, and record item disbursement

3.) Compliance Reporting: Built specialized reports for auditors tracking reservation patterns, pickup compliance, and item movement

4.) Web Accessibility: Transitioned from desktop-only to web-based access for operational staff

## Technical Approach
1.) **Legacy System Analysis:** Reverse-engineered undocumented VB.NET WinForms application to extract and preserve critical business rules and workflows

2.) **Incremental Modernization:** Executed phased migration to ASP.NET Core MVC, prioritizing business continuity and team adoption over architectural purity

3.) **Pragmatic Architecture:** Implemented Dependency Injection and Interface-Service patterns for high-impact components (∼30% of codebase) including audit logging and reporting, establishing foundation for future enhancements

4.) **Evolutionary Data Strategy:** Maintained existing database integrity while strategically extending schema with new audit capabilities to meet compliance requirements

## Key Achievement
Successfully modernized a critical business system independently, transforming an undocumented legacy application into a web-based platform with comprehensive audit trails specifically for the reservation/pickup workflow, providing unprecedented visibility into item allocation for both clerks and auditors.


### To Test out the Demo Version
#### Option 1: Visual Studio (Recommended)
1. Clone this repository
2. Open `FerramentariaTest.sln` in Visual Studio
3. Set startup profile:
   - Right-click the project → Properties → Debug
   - Check that Profile is set to Demo

4. Press **F5** or click **Run**

## Application URL
** http://localhost:5296/

🎯 Demo Features
✅ User Authentication & Authorization

✅ CRUD Operations

✅ In-Memory Database with sample data

## Demo Login
- Username: admin.demo
- Password: admin123

Backend
ASP.NET Core 7.0
Entity Framework Core
In-Memory Database (Demo)
SQL Server (Production)

Frontend
ASP.NET Core MVC
Razor Pages
Bootstrap
JavaScript

Architecture
Repository Pattern
Dependency Injection
Service Layer Architecture