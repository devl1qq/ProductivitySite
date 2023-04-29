import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import RegistrationForm from './components/RegistrationForm';
import LoginForm from './components/LoginForm';
import TaskForm from './components/TaskForm';

const App = () => {
  return (
    <Router>
      <Routes>
        <Route exact path="/" element={<RegistrationForm />} />
        <Route exact path="/login" element={<LoginForm />} />
        <Route exact path="/tasks" element={<TaskForm />} />
      </Routes>
    </Router>
  );
};

export default App;
