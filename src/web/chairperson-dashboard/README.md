# Digital Stokvel Banking - Chairperson Dashboard

React + TypeScript + Vite web dashboard for stokvel group chairpersons.

## Features

- **Group Management**: Create and manage stokvel groups
- **Member Roster**: View and manage group members with roles
- **Contribution Tracking**: Real-time contribution monitoring
- **Payout Approval**: Dual-approval payout workflow
- **Financial Reporting**: Charts and analytics with Chart.js
- **Ledger Export**: Export group ledger as PDF
- **Multi-language**: Support for 5 South African languages

## Prerequisites

- Node.js 18+
- npm or yarn

## Setup

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Environment Variables

Copy `.env.example` to `.env`:

```
VITE_API_BASE_URL=http://localhost:7001/api
```

## Project Structure

```
src/
├── components/       # Reusable UI components
├── pages/            # Page components (Dashboard, Groups, Members, etc.)
├── services/         # API clients
└── i18n/             # Internationalization
```

## Development

- **Dev Server**: Runs on http://localhost:5173
- **API Proxy**: Proxies `/api` requests to backend at http://localhost:7001
- **HMR**: Hot Module Replacement enabled
- **TypeScript**: Strict mode enabled

## Building for Production

```bash
npm run build
```

Output is generated in `dist/` folder.

## Tech Stack

- React 18+
- TypeScript
- Vite 6
- TanStack Query (React Query)
- React Router 7
- Chart.js
- i18next
- Axios
- jsPDF

## License

Proprietary - All rights reserved.
