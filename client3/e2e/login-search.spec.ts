import { test, expect } from '@playwright/test'

test.describe('Login and Search Flow', () => {
  test('should allow user to navigate, search, and interact with chatbot', async ({ page }) => {
    // Navigate to homepage
    await page.goto('/')
    
    // Check if homepage loads correctly
    await expect(page.getByText('Find the Best Deals')).toBeVisible()
    
    // Test search functionality
    await page.fill('input[placeholder*="Search for products"]', 'laptop')
    await page.press('input[placeholder*="Search for products"]', 'Enter')
    
    // Should navigate to products page
    await expect(page).toHaveURL(/.*products.*q=laptop/)
    
    // Check if search results are displayed (or loading state)
    await expect(page.getByText(/Search Results for "laptop"|Loading/)).toBeVisible()
    
    // Test navigation to login page
    await page.click('text=Login')
    await expect(page).toHaveURL(/.*auth\/login/)
    
    // Check login form
    await expect(page.getByText('Welcome back')).toBeVisible()
    await expect(page.getByPlaceholder('Enter your email')).toBeVisible()
    await expect(page.getByPlaceholder('Enter your password')).toBeVisible()
    
    // Test navigation to register page
    await page.click('text=Sign up')
    await expect(page).toHaveURL(/.*auth\/register/)
    
    // Check register form
    await expect(page.getByText('Create account')).toBeVisible()
    await expect(page.getByPlaceholder('Enter your email')).toBeVisible()
    
    // Go back to homepage
    await page.goto('/')
    
    // Test chatbot modal
    await page.click('[data-testid="chatbot-trigger"], button:has-text("💬"), button >> svg')
    
    // Check if chatbot modal opens (might need to adjust selector based on implementation)
    await expect(page.getByText(/Shopping Assistant|Hi! I'm your shopping assistant/)).toBeVisible()
  })
  
  test('should be responsive on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 })
    
    await page.goto('/')
    
    // Check if mobile navigation works
    await expect(page.getByText('Cheapp')).toBeVisible()
    
    // Test mobile search
    await page.fill('input[placeholder*="Search for products"]', 'phone')
    await page.press('input[placeholder*="Search for products"]', 'Enter')
    
    await expect(page).toHaveURL(/.*products.*q=phone/)
  })
})