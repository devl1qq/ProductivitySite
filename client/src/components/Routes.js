import React from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import Routes from './components/Routes';
import RegistrationForm from './components/RegistrationForm';
import LoginForm from './components/LoginForm';

function App() {
  return (
    <Router>
      <Routes exact path="/" component={RegistrationForm} />
      <Routes exact path="/login" component={LoginForm} />
    </Router>
  );
}

export default App;
