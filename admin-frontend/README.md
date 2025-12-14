# Admin Frontend

Modern React-based admin panel for E-Commerce platform.

## Tech Stack

- React 18 + TypeScript
- Vite
- TailwindCSS
- TanStack Query
- React Router
- Axios
- Zustand

## Getting Started

### Prerequisites

- Node.js 18+ (LTS recommended)
- npm or yarn

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

### Environment Variables

Create a `.env` file in the root directory:

```env
VITE_API_URL=http://localhost:5048/api/v1
```

## Features

- ✅ JWT Authentication
- ✅ Dashboard with statistics
- ✅ Product management
- ✅ Order management
- ✅ Responsive design
- ✅ Dark mode support (coming soon)

## Project Structure

```
src/
├── api/              # API client & services
├── components/       # Reusable components
├── features/         # Feature modules
├── layouts/          # Layout components
├── lib/              # Utilities
├── types/            # TypeScript types
└── main.tsx          # Entry point
```

## Development

The app runs on `http://localhost:5173` by default.

API requests are proxied to `http://localhost:5048` (backend API).

## License

MIT
