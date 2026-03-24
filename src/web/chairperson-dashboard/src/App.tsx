import { useState } from 'react'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <div className="container">
        <h1>Digital Stokvel Banking</h1>
        <h2>Chairperson Dashboard</h2>
        <p className="subtitle">
          Manage your stokvel groups, track contributions, and oversee payouts
        </p>
        <div className="card">
          <p>Phase 0 (Foundation) - Dashboard Placeholder</p>
          <p>Version 1.0.0-alpha</p>
        </div>
        <div className="features">
          <h3>Features Coming Soon:</h3>
          <ul>
            <li>Group Management</li>
            <li>Member Roster</li>
            <li>Contribution Tracking</li>
            <li>Payout Approval</li>
            <li>Ledger Export (PDF)</li>
            <li>Financial Reporting</li>
          </ul>
        </div>
      </div>
    </>
  )
}

export default App
